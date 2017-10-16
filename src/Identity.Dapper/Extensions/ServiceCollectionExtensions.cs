﻿using Identity.Dapper.Connections;
using Identity.Dapper.Cryptography;
using Identity.Dapper.Entities;
using Identity.Dapper.Models;
using Identity.Dapper.Repositories;
using Identity.Dapper.Repositories.Contracts;
using Identity.Dapper.Stores;
using Identity.Dapper.UnitOfWork.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Identity.Dapper
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureDapperIdentityCryptography(this IServiceCollection services, IConfigurationSection configuration)
        {
            services.Configure<AESKeys>(configuration);
            services.AddSingleton<EncryptionHelper>();

            return services;
        }

        public static IServiceCollection ConfigureDapperIdentityOptions(this IServiceCollection services, DapperIdentityOptions options)
        {
            services.AddSingleton(options);

            return services;
        }

        public static IdentityBuilder AddDapperIdentityFor<T>(this IdentityBuilder builder) 
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration, T>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType);

            return builder;
        }

        public static IdentityBuilder AddDapperIdentityFor<T>(this IdentityBuilder builder, T configurationOverride)
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration>(configurationOverride);
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType);

            return builder;
        }

        public static IdentityBuilder AddDapperIdentityFor<T, TKey>(this IdentityBuilder builder)
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration, T>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TKey));

            return builder;
        }

        public static IdentityBuilder AddDapperIdentityFor<T, TKey>(this IdentityBuilder builder, T configurationOverride)
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration>(configurationOverride);
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TKey));

            return builder;
        }

        public static IdentityBuilder AddDapperIdentityFor<T, TKey, TUserRole, TRoleClaim>(this IdentityBuilder builder)
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration, T>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TKey), typeof(TUserRole), typeof(TRoleClaim));

            return builder;
        }

        public static IdentityBuilder AddDapperIdentityFor<T, TKey, TUserRole, TRoleClaim>(this IdentityBuilder builder, T configurationOverride)
            where T : SqlConfiguration
        {
            builder.Services.AddSingleton<SqlConfiguration>(configurationOverride);
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

            AddStores(builder.Services, builder.UserType, builder.RoleType, typeof(TKey), typeof(TUserRole), typeof(TRoleClaim));

            return builder;
        }

        private static void AddStores(IServiceCollection services, Type userType, Type roleType, Type keyType = null, Type userRoleType = null, Type roleClaimType = null, Type userClaimType = null, Type userLoginType = null)
        {
            Type userStoreType;
            Type roleStoreType;
            keyType = keyType ?? typeof(int);
            userRoleType = userRoleType ?? typeof(DapperIdentityUserRole<>).MakeGenericType(keyType);
            roleClaimType = roleClaimType ?? typeof(DapperIdentityRoleClaim<>).MakeGenericType(keyType);
            userClaimType = userClaimType ?? typeof(DapperIdentityUserClaim<>).MakeGenericType(keyType);
            userLoginType = userLoginType ?? typeof(DapperIdentityUserLogin<>).MakeGenericType(keyType);

            userStoreType = typeof(DapperUserStore<,,,,,,>).MakeGenericType(userType, keyType, userRoleType, roleClaimType,
                                                                           userClaimType, userLoginType, roleType);
            roleStoreType = typeof(DapperRoleStore<,,,>).MakeGenericType(roleType, keyType, userRoleType, roleClaimType);

            services.AddScoped(typeof(IRoleRepository<,,,>).MakeGenericType(roleType, keyType, userRoleType, roleClaimType),
                               typeof(RoleRepository<,,,>).MakeGenericType(roleType, keyType, userRoleType, roleClaimType));

            services.AddScoped(typeof(IUserRepository<,,,,,,>).MakeGenericType(userType, keyType, userRoleType,
                                                                              roleClaimType, userClaimType,
                                                                              userLoginType, roleType),
                               typeof(UserRepository<,,,,,,>).MakeGenericType(userType, keyType, userRoleType,
                                                                             roleClaimType, userClaimType,
                                                                             userLoginType, roleType));

            services.AddScoped(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);
            services.AddScoped(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
        }

        public static IServiceCollection ConfigureDapperConnectionProvider<T>(this IServiceCollection services, IConfigurationSection configuration)
            where T : class, IConnectionProvider
        {
            services.Configure<ConnectionProviderOptions>(configuration);

            services.AddScoped<IConnectionProvider, T>();

            return services;
        }
    }
}
