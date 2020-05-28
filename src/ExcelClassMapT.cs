﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using ExcelMapper.Utilities;
using ExcelMapper.Mappers;
using ExcelMapper.Readers;
using ExcelMapper.Abstractions;

namespace ExcelMapper
{
    /// <summary>
    /// A map that maps a row of a sheet to an object of the given type.
    /// </summary>
    /// <typeparam name="T">The typ eof the object to create.</typeparam>
    public class ExcelClassMap<T> : ExcelClassMap
    {
        /// <summary>
        /// Gets the default strategy to use when the value of a cell is empty.
        /// </summary>
        public FallbackStrategy EmptyValueStrategy { get; }

        /// <summary>
        /// Constructs the default class map for the given type.
        /// </summary>
        public ExcelClassMap() : base(typeof(T)) { }

        /// <summary>
        /// Constructs a new class map for the given type using the given default strategy to use
        /// when the value of a cell is empty.
        /// </summary>
        /// <param name="emptyValueStrategy">The default strategy to use when the value of a cell is empty.</param>
        public ExcelClassMap(FallbackStrategy emptyValueStrategy) : this()
        {
            if (!Enum.IsDefined(typeof(FallbackStrategy), emptyValueStrategy))
            {
                throw new ArgumentException($"Invalid value \"{emptyValueStrategy}\".", nameof(emptyValueStrategy));
            }

            EmptyValueStrategy = emptyValueStrategy;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping primitives such as string, int etc.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public OneToOnePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            if (!AutoMapper.TryCreatePrimitiveMap(memberExpression.Member, EmptyValueStrategy, out OneToOnePropertyMap<TProperty> map))
            {
                throw new ExcelMappingException($"Don't know how to map type {typeof(TProperty)}.");
            }

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping enums.
        /// </summary>
        /// <typeparam name="TProperty">The enum type of the property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <param name="ignoreCase">A flag indicating whether enum parsing is case insensitive.</param>
        /// <returns>The map for the given property or field.</returns>
        public OneToOnePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, TProperty>> expression, bool ignoreCase) where TProperty : struct
        {
            if (!typeof(TProperty).GetTypeInfo().IsEnum)
            {
                throw new ArgumentException($"The type ${typeof(TProperty)} must be an Enum.", nameof(TProperty));
            }

            MemberExpression memberExpression = GetMemberExpression(expression);
            var mapper = new EnumMapper(typeof(TProperty), ignoreCase);
            ISingleCellValueReader defaultReader = AutoMapper.GetDefaultSingleCellValueReader(memberExpression.Member);
            var map = new OneToOnePropertyMap<TProperty>(memberExpression.Member, defaultReader)
                .WithCellValueMappers(mapper)
                .WithThrowingEmptyFallback()
                .WithThrowingInvalidFallback();

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping nullable enums.
        /// </summary>
        /// <typeparam name="TProperty">The enum type of the property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <param name="ignoreCase">A flag indicating whether enum parsing is case insensitive.</param>
        /// <returns>The map for the given property or field.</returns>
        public OneToOnePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, TProperty?>> expression, bool ignoreCase) where TProperty : struct
        {
            if (!typeof(TProperty).GetTypeInfo().IsEnum)
            {
                throw new ArgumentException($"The type ${typeof(TProperty)} must be an Enum.", nameof(TProperty));
            }

            MemberExpression memberExpression = GetMemberExpression(expression);
            var mapper = new EnumMapper(typeof(TProperty), ignoreCase);
            ISingleCellValueReader defaultReader = AutoMapper.GetDefaultSingleCellValueReader(memberExpression.Member);
            var map = new OneToOnePropertyMap<TProperty>(memberExpression.Member, defaultReader)
                .WithCellValueMappers(mapper)
                .WithEmptyFallback(null)
                .WithThrowingInvalidFallback();

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping enumerables.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, IEnumerable<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping ICollections.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, ICollection<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping Collection.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, Collection<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping ObservableCollection.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, ObservableCollection<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping ILists.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, IList<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> mapping = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(mapping, expression);
            return mapping;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping lists.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, List<TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping arrays.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneEnumerablePropertyMap<TProperty> Map<TProperty>(Expression<Func<T, TProperty[]>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            ManyToOneEnumerablePropertyMap<TProperty> map = GetMultiMap<TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping objects that contain nested objects, primitives or enumerables.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneObjectPropertyMap<TProperty> MapObject<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            if (!AutoMapper.TryCreateObjectMap(memberExpression.Member, EmptyValueStrategy, out ManyToOneObjectPropertyMap<TProperty> map))
            {
                throw new ExcelMappingException($"Could not map object of type \"{typeof(TProperty)}\".");
            }

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping IDictionarys.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneDictionaryPropertyMap<TProperty> Map<TProperty>(Expression<Func<T, IDictionary<string, TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            var map = GetDictionaryMap<string, TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping IDictionarys.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneDictionaryPropertyMap<TProperty> Map<TProperty>(Expression<Func<T, Dictionary<string, TProperty>>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            var map = GetDictionaryMap<string, TProperty>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        /// <summary>
        /// Creates a map for a property or field given a MemberExpression reading the property or field.
        /// This is used for mapping ExpandoObjects.
        /// </summary>
        /// <typeparam name="TProperty">The element type of property or field to map.</typeparam>
        /// <param name="expression">A MemberExpression reading the property or field.</param>
        /// <returns>The map for the given property or field.</returns>
        public ManyToOneDictionaryPropertyMap<object> Map(Expression<Func<T, ExpandoObject>> expression)
        {
            MemberExpression memberExpression = GetMemberExpression(expression);
            var map = GetDictionaryMap<string, object>(memberExpression.Member);

            AddMap(map, expression);
            return map;
        }

        private ManyToOneEnumerablePropertyMap<TProperty> GetMultiMap<TProperty>(MemberInfo member)
        {
            if (!AutoMapper.TryCreateGenericEnumerableMap(member, EmptyValueStrategy, out ManyToOneEnumerablePropertyMap<TProperty> map))
            {
                throw new ExcelMappingException($"No known way to instantiate type \"{typeof(TProperty)}\". It must be a single dimensional array, be assignable from List<T> or implement ICollection<T>.");
            }

            return map;
        }

        private ManyToOneDictionaryPropertyMap<TValue> GetDictionaryMap<TKey, TValue>(MemberInfo member)
        {
            if (!AutoMapper.TryCreateGenericDictionaryMap<TKey, TValue>(member, EmptyValueStrategy, out ManyToOneDictionaryPropertyMap<TValue> mapping))
            {
                throw new ExcelMappingException($"No known way to instantiate type \"IDictionary<{typeof(TKey)}, {typeof(TValue)}>\".");
            }

            return mapping;
        }

        protected internal MemberExpression GetMemberExpression<TProperty>(Expression<Func<T, TProperty>> expression)
        {
            if (!(expression.Body is MemberExpression rootMemberExpression))
            {
                throw new ArgumentException("Not a member expression.", nameof(expression));
            }

            return rootMemberExpression;
        }

        protected internal void AddMap<TProperty>(ExcelPropertyMap map, Expression<Func<T, TProperty>> expression)
        {
            Expression expressionBody = expression.Body;
            var expressions = new Stack<MemberExpression>();
            while (true)
            {
                if (!(expressionBody is MemberExpression memberExpressionBody))
                {
                    // Each mapping is of the form (parameter => member).
                    if (expressionBody is ParameterExpression _)
                    {
                        break;
                    }

                    throw new ArgumentException($"Expression can only contain member accesses, but found {expressionBody}.", nameof(expression));
                }

                expressions.Push(memberExpressionBody);
                expressionBody = memberExpressionBody.Expression;
            }

            if (expressions.Count == 1)
            {
                // Simple case: parameter => prop
                Mappings.Add(map);
            }
            else
            {
                // Go through the chain of members and make sure that they are valid.
                // E.g. parameter => parameter.prop.subprop.field.
                CreateObjectMap(map, expressions);
            }
        }
    }
}
