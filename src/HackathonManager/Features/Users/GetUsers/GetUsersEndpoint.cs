using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints.AspVersioning;
using FastIDs.TypeId;
using HackathonManager.Exceptions;
using HackathonManager.Extensions;
using HackathonManager.Persistence;
using HackathonManager.Persistence.Entities;
using HackathonManager.Utilities;
using HackathonManager.Utilities.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using NodaTime;
using Sqids;

namespace HackathonManager.Features.Users.GetUsers;

[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static")]
public sealed class GetUsersEndpoint(HackathonDbContext _dbContext, SqidsEncoder<uint> _encoder)
    : PaginatedEndpoint<GetUsersRequest, Results<Ok<GetUsersResponseDto>, NotModified>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/users");
        AllowAnonymous();

        Description(b =>
        {
            b.WithVersionSet(ApiTags.Users).HasApiVersion(1);
            b.Produces(StatusCodes.Status304NotModified);
        });
    }

    /// <inheritdoc />
    public override async Task<Results<Ok<GetUsersResponseDto>, NotModified>> ExecuteAsync(
        GetUsersRequest req,
        CancellationToken ct
    )
    {
        uint? requestETag = null;
        if (req.IfNoneMatch is not null && !_encoder.DecodeAndValidate(req.IfNoneMatch, out requestETag))
        {
            this.ThrowInvalidETagError(HeaderNames.IfNoneMatch);
        }

        if (TryDecodeCursor<GetUsersCursor>(req.Cursor, out var cursor))
        {
            req = req with
            {
                Search = cursor.Search,
                Term = cursor.SearchTerm,
                Sort = cursor.Sort,
                PageSize = cursor.PageSize,
            };
        }

        var query = _dbContext.Users.AsNoTracking();
        query = ApplyCursor(query, cursor);
        query = ApplySearchFilter(query, req);
        query = ApplySorting(query, req.Sort);

        var users = await query.Take(req.PageSize + 1).ToArrayAsync(ct);
        var etag = users.Take(req.PageSize).GenerateETag();

        if (requestETag == etag)
        {
            return NotModified.Instance;
        }

        HttpContext.Response.Headers.ETag = new StringValues(_encoder.Encode(etag));
        var response = new GetUsersResponseDto(
            users.Take(req.PageSize).Select(u => u.ToDto(_encoder)),
            cursor is null ? null : PaginationCursor.Encode(cursor),
            CreateNextCursor(req, users)
        );

        return TypedResults.Ok(response);
    }

    private IQueryable<User> ApplyCursor(IQueryable<User> query, GetUsersCursor? cursor)
    {
        if (cursor is null)
        {
            return query;
        }

        if (cursor.Sort is UserSort.Created or UserSort.CreatedDesc)
        {
            var valueParts = cursor.SortValue.Split('|');
            if (valueParts.Length is not 2)
            {
                ThrowInvalidCursor();
            }

            var created = Instant.FromDateTimeOffset(DateTimeOffset.Parse(valueParts[0], CultureInfo.InvariantCulture));
            var typeId = TypeId.Parse(valueParts[1]).Decode();
            const string idProperty = nameof(Persistence.Entities.User.Id);

            return query.Where(u =>
                (u.CreatedAt == created && EF.Property<Guid>(u, idProperty) > typeId.Id)
                || (cursor.Sort == UserSort.CreatedDesc ? u.CreatedAt > created : u.CreatedAt < created)
            );
        }

        return cursor.Sort switch
        {
            UserSort.Email => query.Where(u => u.Email.CompareTo(cursor.SortValue) > 0),
            UserSort.EmailDesc => query.Where(u => u.Email.CompareTo(cursor.SortValue) < 0),
            UserSort.Name => query.Where(u => u.DisplayName.CompareTo(cursor.SortValue) > 0),
            UserSort.NameDesc => query.Where(u => u.DisplayName.CompareTo(cursor.SortValue) < 0),
            _ => throw new UnexpectedEnumValueException<UserSort>(cursor.Sort),
        };
    }

    private IQueryable<User> ApplySearchFilter(IQueryable<User> query, GetUsersRequest? request)
    {
        if (request is not { Search: not null, Term.Length: > 0 })
        {
            return query;
        }

        var pattern = $"%{request.Term}%";
        return request.Search switch
        {
            UserSearch.Email => query.Where(u => EF.Functions.ILike(u.Email, pattern)),
            UserSearch.Name => query.Where(u => EF.Functions.ILike(u.DisplayName, pattern)),
            _ => throw new UnexpectedEnumValueException<UserSearch>(request.Search),
        };
    }

    private IOrderedQueryable<User> ApplySorting(IQueryable<User> query, UserSort sort)
    {
        return sort switch
        {
            UserSort.Created => query.OrderBy(u => u.CreatedAt).ThenBy(u => u.Id),
            UserSort.CreatedDesc => query.OrderByDescending(u => u.CreatedAt).ThenBy(u => u.Id),
            UserSort.Email => query.OrderBy(u => u.Email),
            UserSort.EmailDesc => query.OrderByDescending(u => u.Email),
            UserSort.Name => query.OrderBy(u => u.DisplayName),
            UserSort.NameDesc => query.OrderByDescending(u => u.DisplayName),
            _ => throw new UnexpectedEnumValueException<UserSort>(sort),
        };
    }

    private string? CreateNextCursor(GetUsersRequest request, User[] users)
    {
        if (users.Length <= request.PageSize)
        {
            return null;
        }

        var lastUser = users[^1];
        var lastSortValue = request.Sort switch
        {
            UserSort.Created or UserSort.CreatedDesc => $"{lastUser.CreatedAt}|{lastUser.Id}",
            UserSort.Email or UserSort.EmailDesc => lastUser.Email,
            UserSort.Name or UserSort.NameDesc => lastUser.DisplayName,
            _ => throw new UnexpectedEnumValueException<UserSort>(request.Sort),
        };

        var cursor = new GetUsersCursor(request.Search, request.Term, request.Sort, request.PageSize, lastSortValue);
        return PaginationCursor.Encode(cursor);
    }
}
