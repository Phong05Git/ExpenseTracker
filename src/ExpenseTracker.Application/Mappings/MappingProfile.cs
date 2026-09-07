using AutoMapper;
using ExpenseTracker.Application.DTOs.Budgets;
using ExpenseTracker.Application.DTOs.Categories;
using ExpenseTracker.Application.DTOs.Transactions;
using ExpenseTracker.Application.DTOs.Users;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.Icon))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color));

        CreateMap<Category, CategoryDto>();

        CreateMap<Budget, BudgetDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
            .ForMember(dest => dest.CategoryIcon, opt => opt.MapFrom(src => src.Category.Icon))
            .ForMember(dest => dest.CategoryColor, opt => opt.MapFrom(src => src.Category.Color));

        CreateMap<User, UserProfileDto>();
    }
}