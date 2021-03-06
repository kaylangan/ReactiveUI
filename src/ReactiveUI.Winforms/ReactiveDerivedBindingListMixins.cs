﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using ReactiveUI.Legacy;

namespace ReactiveUI.Winforms.Legacy
{
    /// <summary>
    /// IReactiveDerivedList represents a bindinglist whose contents will "follow" another
    /// collection; this method is useful for creating ViewModel collections
    /// that are automatically updated when the respective Model collection is updated.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    [Obsolete("ReactiveList is no longer supported. We suggest replacing it with DynamicData https://github.com/rolandpheasant/dynamicdata")]
    public interface IReactiveDerivedBindingList<T> : IReactiveDerivedList<T>, IBindingList
    {
    }

    [Obsolete("ReactiveList is no longer supported. We suggest replacing it with DynamicData https://github.com/rolandpheasant/dynamicdata")]
    internal class ReactiveDerivedBindingList<TSource, TValue> :
        ReactiveDerivedCollection<TSource, TValue>, IReactiveDerivedBindingList<TValue>
    {
        public ReactiveDerivedBindingList(
            IEnumerable<TSource> source,
            Func<TSource, TValue> selector,
            Func<TSource, bool> filter,
            Func<TValue, TValue, int> orderer,
            Action<TValue> removed,
            IObservable<Unit> signalReset)
            : base(source, selector, filter, orderer, removed, signalReset, Scheduler.Immediate)
        {
        }

        protected override void RaiseCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.RaiseCollectionChanged(e);
            if (ListChanged != null)
            {
                e.AsListChangedEventArgs().ForEach(x => ListChanged(this, x));
            }
        }

        private const string ReadonlyExceptionMessage = "Derived collections cannot be modified.";

        public object AddNew()
        {
            throw new NotSupportedException(ReadonlyExceptionMessage);
        }

        public void AddIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        public int Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        public void RemoveIndex(PropertyDescriptor property)
        {
            throw new NotSupportedException();
        }

        public void RemoveSort()
        {
            throw new NotSupportedException();
        }

        public bool AllowNew => false;

        public bool AllowEdit => false;

        public bool AllowRemove => false;

        public bool SupportsChangeNotification => true;

        public bool SupportsSearching => false;

        public bool SupportsSorting => false;

        public bool IsSorted => false;

        public PropertyDescriptor SortProperty => null;

        public ListSortDirection SortDirection => ListSortDirection.Ascending;

        public event ListChangedEventHandler ListChanged;
    }

    [Obsolete("ReactiveList is no longer supported. We suggest replacing it with DynamicData https://github.com/rolandpheasant/dynamicdata")]
#pragma warning disable SA1600 // Elements should be documented
    public static class ObservableCollectionMixin
    {
        /// <summary>
        /// Creates a collection whose contents will "follow" another
        /// collection; this method is useful for creating ViewModel collections
        /// that are automatically updated when the respective Model collection
        /// is updated.
        ///
        /// Note that even though this method attaches itself to any
        /// IEnumerable, it will only detect changes from objects implementing
        /// INotifyCollectionChanged (like ReactiveList). If your source
        /// collection doesn't implement this, signalReset is the way to signal
        /// the derived collection to reorder/refilter itself.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <typeparam name="TNew">The new type.</typeparam>
        /// <typeparam name="TDontCare">The signal type.</typeparam>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="removed">An action that is called on each item when
        /// it is removed.</param>
        /// <param name="filter">A filter to determine whether to exclude items
        /// in the derived collection.</param>
        /// <param name="orderer">A comparator method to determine the ordering of
        /// the resulting collection.</param>
        /// <param name="signalReset">When this Observable is signalled,
        /// the derived collection will be manually
        /// reordered/refiltered.</param>
        /// <param name="this">The source collection to follow.</param>
        /// <returns>A new collection whose items are equivalent to
        /// Collection.Select().Where().OrderBy() and will mirror changes
        /// in the initial collection.</returns>
        public static IReactiveDerivedBindingList<TNew> CreateDerivedBindingList<T, TNew, TDontCare>(
            this IEnumerable<T> @this,
            Func<T, TNew> selector,
            Action<TNew> removed,
            Func<T, bool> filter = null,
            Func<TNew, TNew, int> orderer = null,
            IObservable<TDontCare> signalReset = null)
        {
            Contract.Requires(selector != null);

            IObservable<Unit> reset = null;

            if (signalReset != null)
            {
                reset = signalReset.Select(_ => Unit.Default);
            }

            return new ReactiveDerivedBindingList<T, TNew>(@this, selector, filter, orderer, removed, reset);
        }

        /// <summary>
        /// Creates a collection whose contents will "follow" another
        /// collection; this method is useful for creating ViewModel collections
        /// that are automatically updated when the respective Model collection
        /// is updated.
        ///
        /// Note that even though this method attaches itself to any
        /// IEnumerable, it will only detect changes from objects implementing
        /// INotifyCollectionChanged (like ReactiveList). If your source
        /// collection doesn't implement this, signalReset is the way to signal
        /// the derived collection to reorder/refilter itself.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <typeparam name="TNew">The new type.</typeparam>
        /// <typeparam name="TDontCare">The signal type.</typeparam>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="filter">A filter to determine whether to exclude items
        /// in the derived collection.</param>
        /// <param name="orderer">A comparator method to determine the ordering of
        /// the resulting collection.</param>
        /// <param name="signalReset">When this Observable is signalled,
        /// the derived collection will be manually
        /// reordered/refiltered.</param>
        /// <param name="this">The source collection to follow.</param>
        /// <returns>A new collection whose items are equivalent to
        /// Collection.Select().Where().OrderBy() and will mirror changes
        /// in the initial collection.</returns>
        public static IReactiveDerivedBindingList<TNew> CreateDerivedBindingList<T, TNew, TDontCare>(
            this IEnumerable<T> @this,
            Func<T, TNew> selector,
            Func<T, bool> filter = null,
            Func<TNew, TNew, int> orderer = null,
            IObservable<TDontCare> signalReset = null)
        {
            return @this.CreateDerivedBindingList(selector, null, filter, orderer, signalReset);
        }

        /// <summary>
        /// Creates a collection whose contents will "follow" another
        /// collection; this method is useful for creating ViewModel collections
        /// that are automatically updated when the respective Model collection
        /// is updated.
        ///
        /// Be aware that this overload will result in a collection that *only*
        /// updates if the source implements INotifyCollectionChanged. If your
        /// list changes but isn't a ReactiveList/ObservableCollection,
        /// you probably want to use the other overload.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <typeparam name="TNew">The new type.</typeparam>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="removed">An action that is called on each item when
        /// it is removed.</param>
        /// <param name="filter">A filter to determine whether to exclude items
        /// in the derived collection.</param>
        /// <param name="orderer">A comparator method to determine the ordering of
        /// the resulting collection.</param>
        /// <param name="this">The source collection to follow.</param>
        /// <returns>A new collection whose items are equivalent to
        /// Collection.Select().Where().OrderBy() and will mirror changes
        /// in the initial collection.</returns>
        public static IReactiveDerivedBindingList<TNew> CreateDerivedBindingList<T, TNew>(
            this IEnumerable<T> @this,
            Func<T, TNew> selector,
            Action<TNew> removed,
            Func<T, bool> filter = null,
            Func<TNew, TNew, int> orderer = null)
        {
            return @this.CreateDerivedBindingList(selector, removed, filter, orderer, (IObservable<Unit>)null);
        }

        /// <summary>
        /// Creates a collection whose contents will "follow" another
        /// collection; this method is useful for creating ViewModel collections
        /// that are automatically updated when the respective Model collection
        /// is updated.
        ///
        /// Be aware that this overload will result in a collection that *only*
        /// updates if the source implements INotifyCollectionChanged. If your
        /// list changes but isn't a ReactiveList/ObservableCollection,
        /// you probably want to use the other overload.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <typeparam name="TNew">The new type.</typeparam>
        /// <param name="selector">A Select function that will be run on each
        /// item.</param>
        /// <param name="filter">A filter to determine whether to exclude items
        /// in the derived collection.</param>
        /// <param name="orderer">A comparator method to determine the ordering of
        /// the resulting collection.</param>
        /// <param name="this">The source collection to follow.</param>
        /// <returns>A new collection whose items are equivalent to
        /// Collection.Select().Where().OrderBy() and will mirror changes
        /// in the initial collection.</returns>
        public static IReactiveDerivedBindingList<TNew> CreateDerivedBindingList<T, TNew>(
            this IEnumerable<T> @this,
            Func<T, TNew> selector,
            Func<T, bool> filter = null,
            Func<TNew, TNew, int> orderer = null)
        {
            return @this.CreateDerivedBindingList(selector, null, filter, orderer, (IObservable<Unit>)null);
        }
    }

#pragma warning restore SA1600 // Elements should be documented
}
