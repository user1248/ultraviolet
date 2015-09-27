﻿using System;
using TwistedLogik.Nucleus;
using TwistedLogik.Ultraviolet.Input;
using TwistedLogik.Ultraviolet.UI.Presentation.Controls.Primitives;
using TwistedLogik.Ultraviolet.UI.Presentation.Input;
using TwistedLogik.Ultraviolet.UI.Presentation.Media;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Controls
{
    /// <summary>
    /// Represents a list of selectable items.
    /// </summary>
    [UvmlKnownType(null, "TwistedLogik.Ultraviolet.UI.Presentation.Controls.Templates.ListBox.xml")]
    public class ListBox : Selector
    {
        /// <summary>
        /// Initializes the <see cref="ListBox"/> type.
        /// </summary>
        static ListBox()
        {
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ListBox), new PropertyMetadata<KeyboardNavigationMode>(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ListBox), new PropertyMetadata<KeyboardNavigationMode>(KeyboardNavigationMode.Once));
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(ListBox), new PropertyMetadata<Boolean>(CommonBoxedValues.Boolean.False));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBox"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="name">The element's identifying name within its namescope.</param>
        public ListBox(UltravioletContext uv, String name)
            : base(uv, name)
        {
            SetValue<ListBoxSelectedItems>(SelectedItemsPropertyKey, selectedItems);
        }

        /// <summary>
        /// Gets or sets the list box's selection mode.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get { return GetValue<SelectionMode>(SelectionModeProperty); }
            set { SetValue<SelectionMode>(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Gets the list box's collection of selected items.
        /// </summary>
        public ListBoxSelectedItems SelectedItems
        {
            get { return GetValue<ListBoxSelectedItems>(SelectedItemsProperty); }
        }

        /// <summary>
        /// Identifies the <see cref="SelectionMode"/> dependency property.
        /// </summary>
        /// <remarks>The styling name of this dependency property is 'selection-mode'.</remarks>
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(SelectionMode), typeof(ListBox),
            new PropertyMetadata<SelectionMode>(SelectionMode.Single));

        /// <summary>
        /// The private access key for the <see cref="SelectedItems"/> read-only dependency property.
        /// </summary>
        private static readonly DependencyPropertyKey SelectedItemsPropertyKey = DependencyProperty.RegisterReadOnly("SelectedItems", typeof(ListBoxSelectedItems), typeof(ListBox),
            new PropertyMetadata<ListBoxSelectedItems>());

        /// <summary>
        /// Identifies the <see cref="SelectedItems"/> dependency property.
        /// </summary>
        /// <remarks>The styling name of this dependency property is 'selected-items'.</remarks>
        public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsPropertyKey.DependencyProperty;

        /// <summary>
        /// Called to inform the list box that one of its items was clicked.
        /// </summary>
        /// <param name="container">The item container that was clicked.</param>
        internal void HandleItemClicked(ListBoxItem container)
        {
            var item = ItemContainerGenerator.ItemFromContainer(container);
            if (item == null)
                return;

            switch (SelectionMode)
            {
                case SelectionMode.Single:
                    HandleItemClickedSingle(item);
                    break;

                case SelectionMode.Multiple:
                    HandleItemClickedMultiple(item);
                    break;
            }
        }

        /// <inheritdoc/>
        protected internal override Panel CreateItemsPanel()
        {
            return new StackPanel(Ultraviolet, null);
        }

        /// <inheritdoc/>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListBoxItem(Ultraviolet, null);
        }

        /// <inheritdoc/>
        protected override Boolean IsItemContainer(DependencyObject element)
        {
            return element is ListBoxItem;
        }

        /// <inheritdoc/>
        protected override Boolean IsItemContainerForItem(DependencyObject container, Object item)
        {
            var lbi = container as ListBoxItem;
            if (lbi == null)
                return false;

            return lbi.Content == item;
        }

        /// <inheritdoc/>
        protected override void OnSelectedItemAdded(Object item)
        {
            selectedItems.Add(item);
            base.OnSelectedItemAdded(item);
        }

        /// <inheritdoc/>
        protected override void OnSelectedItemRemoved(Object item)
        {
            selectedItems.Remove(item);
            base.OnSelectedItemRemoved(item);
        }

        /// <inheritdoc/>
        protected override void OnSelectedItemsChanged()
        {
            selectedItems.Clear();
            foreach (var item in Items)
            {
                var container = ItemContainerGenerator.ContainerFromItem(item);
                if (container != null && GetIsSelected(container))
                {
                    selectedItems.Add(container);
                }
            }

            base.OnSelectedItemsChanged();
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyboardDevice device, Key key, ModifierKeys modifiers, ref RoutedEventData data)
        {
            var selection = Keyboard.GetFocusedElement(View) as UIElement;
            var navdir = (FocusNavigationDirection?)null;

            switch (key)
            {
                case Key.Space:
                case Key.Return:
                    if (key == Key.Return && !GetValue<Boolean>(KeyboardNavigation.AcceptsReturnProperty))
                        break;

                    var listBoxItem = data.OriginalSource as ListBoxItem;
                    if (listBoxItem != null && ItemsControlFromItemContainer(listBoxItem) == this)
                    {
                        HandleItemClicked(listBoxItem);
                        data.Handled = true;
                    }
                    break;

                case Key.Up:
                    if (SelectionMode == SelectionMode.Single)
                    {
                        navdir = FocusNavigationDirection.Up;
                    }
                    break;

                case Key.Down:
                    if (SelectionMode == SelectionMode.Single)
                    {
                        navdir = FocusNavigationDirection.Down;
                    }
                    break;
            }

            if (navdir.HasValue)
            {
                if (selection != null && selection.MoveFocus(navdir.Value))
                {
                    var focused = Keyboard.GetFocusedElement(View) as UIElement;
                    var listBoxItem = FindContainer(focused);
                    if (listBoxItem != null && (modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                    {
                        HandleItemClicked(listBoxItem);
                    }
                    data.Handled = true;
                }
            }

            base.OnKeyDown(device, key, modifiers, ref data);
        }

        /// <summary>
        /// Finds the <see cref="ListBoxItem"/> that contains the specified object.
        /// </summary>
        /// <param name="item">The object for which to find a container.</param>
        /// <returns>The <see cref="ListBoxItem"/> that contains the specified object, or <c>null</c>.</returns>
        private ListBoxItem FindContainer(Object item)
        {
            var current = item as DependencyObject;

            while (current != null)
            {
                if (current == this)
                    return null;

                if (current is ListBoxItem)
                    return ItemsControlFromItemContainer((ListBoxItem)current) == this ? (ListBoxItem)current : null;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        /// <summary>
        /// Handles clicking on an item when the list box is in single selection mode.
        /// </summary>
        /// <param name="item">The item that was clicked.</param>
        private void HandleItemClickedSingle(Object item)
        {
            var dobj = item as DependencyObject;
            if (dobj == null)
                return;

            BeginChangeSelection();

            if (GetIsSelected(dobj))
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    UnselectItem(item);
                }
            }
            else
            {
                UnselectAllItems();
                SelectItem(item);
            }

            EndChangeSelection();
        }

        /// <summary>
        /// Handles clicking on an item when the list box is in multiple selection mode.
        /// </summary>
        /// <param name="item">The item that was clicked.</param>
        private void HandleItemClickedMultiple(Object item)
        {
            var dobj = item as DependencyObject;
            if (dobj == null)
                return;

            var selected = GetIsSelected(dobj);
            if (selected)
            {
                UnselectItem(item);
            }
            else
            {
                SelectItem(item);
            }
        }        

        // Property values.
        private readonly ListBoxSelectedItems selectedItems = 
            new ListBoxSelectedItems();
    }
}
