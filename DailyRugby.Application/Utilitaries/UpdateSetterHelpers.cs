using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace DailyRugby.Application.Utilitaries;

public static class UpdateSetterHelpers
{
    public static UpdateSettersBuilder<TModel> SetPropertyIf<TModel, TProperty>
        (this UpdateSettersBuilder<TModel> updater,
        bool condition,
        Expression<Func<TModel, TProperty>> property,
        TProperty value)
    {
        if (!condition) return updater;
        return updater.SetProperty(property, value);
    }

    public static UpdateSettersBuilder<TModel> SetPropertyIf<TModel, TProperty>
        (this UpdateSettersBuilder<TModel> updater,
        bool condition,
        Expression<Func<TModel, TProperty>> property,
        Expression<Func<TModel, TProperty>> value)
    {
        if (!condition) return updater;
        return updater.SetProperty(property, value);
    }
}