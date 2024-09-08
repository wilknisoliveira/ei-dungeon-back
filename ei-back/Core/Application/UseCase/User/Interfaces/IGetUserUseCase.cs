﻿using ei_back.Core.Application.UseCase.User.Dtos;
using ei_back.Infrastructure.Context;

namespace ei_back.Core.Application.UseCase.User.Interfaces
{
    public interface IGetUserUseCase
    {
        Task<PagedSearchDto<UserGetDtoResponse>> Handler(
            string? name,
            string sortDirection,
            int pageSize,
            int page);
    }
}
