using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Interfaces.UseCases;
using TaskManager.Application.UseCases.Auth;
using TaskManager.Application.UseCases.Tasks;

namespace TaskManager.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Auth Use Cases
        services.AddScoped<IRegisterUserUseCase, RegisterUserUseCase>();
        services.AddScoped<ILoginUserUseCase, LoginUserUseCase>();

        // Task Use Cases
        services.AddScoped<ICreateTaskUseCase, CreateTaskUseCase>();
        services.AddScoped<IGetTasksUseCase, GetTasksUseCase>();
        services.AddScoped<IGetTaskUseCase, GetTaskUseCase>();
        services.AddScoped<IUpdateTaskUseCase, UpdateTaskUseCase>();
        services.AddScoped<IDeleteTaskUseCase, DeleteTaskUseCase>();

        return services;
    }
}
