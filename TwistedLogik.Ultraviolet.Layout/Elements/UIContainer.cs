﻿using System;
using TwistedLogik.Nucleus;
using TwistedLogik.Ultraviolet.Graphics;
using TwistedLogik.Ultraviolet.Graphics.Graphics2D;

namespace TwistedLogik.Ultraviolet.Layout.Elements
{
    /// <summary>
    /// Represents an interface element which can contain other elements.
    /// </summary>
    public abstract class UIContainer : UIElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UIContainer"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="id">The element's unique identifier within its view.</param>
        public UIContainer(UltravioletContext uv, String id)
            : base(uv, id)
        {
            this.children = new UIElementCollection(this);
        }

        /// <summary>
        /// Recursively clears the local values of all of the container's dependency properties
        /// and all of the dependency properties of the container's descendents.
        /// </summary>
        public void ClearLocalValuesRecursive()
        {
            ClearLocalValues();

            foreach (var child in children)
            {
                child.ClearLocalValues();
            }
        }

        /// <summary>
        /// Recursively clears the styled values of all of the container's dependency properties
        /// and all of the dependency properties of the container's descendents.
        /// </summary>
        public void ClearStyledValuesRecursive()
        {
            ClearStyledValues();

            foreach (var child in children)
            {
                child.ClearStyledValues();
            }
        }

        /// <summary>
        /// Requests that a layout be performed during the next call to <see cref="UIElement.Update(UltravioletTime)"/>.
        /// </summary>
        public void RequestLayout()
        {
            layoutRequested = true;
        }

        /// <summary>
        /// Immediately recalculates the layout of the container and all of its children.
        /// </summary>
        public void PerformLayout()
        {
            OnPerformingLayout();

            foreach (var child in children)
            {
                PerformLayoutInternal(child, false);
            }
            DetermineIfScissorRectangleIsRequired();

            OnPerformedLayout();
        }

        /// <summary>
        /// Immediately recalculates the layout of the specified child element.
        /// </summary>
        /// <param name="child">The child element for which to calculate a layout.</param>
        public void PerformLayout(UIElement child)
        {
            Contract.Require(child, "child");
            Contract.Ensure<ArgumentException>(Children.Contains(child), "child");

            PerformLayoutInternal(child, true);
        }

        /// <summary>
        /// Called when the element and its children should reload their content.
        /// </summary>
        public void ReloadContentRecursively()
        {
            ReloadContent();

            foreach (var child in children)
            {
                var container = child as UIContainer;
                if (container != null)
                {
                    container.ReloadContentRecursively();
                }
                else
                {
                    container.ReloadContent();
                }
            }
        }

        /// <summary>
        /// Gets the container's collection of child elements.
        /// </summary>
        public UIElementCollection Children
        {
            get { return children; }
        }

        /// <summary>
        /// Called before the container performs a layout.
        /// </summary>
        protected virtual void OnPerformingLayout()
        {

        }

        /// <summary>
        /// Called after the container has performed a layout.
        /// </summary>
        protected virtual void OnPerformedLayout()
        {

        }

        /// <summary>
        /// Calculates the container-relative layout area of the specified child element.
        /// </summary>
        /// <param name="child">The child element for which to calculate a layout area.</param>
        /// <returns>The container-relative layout area of the specified child element.</returns>
        protected abstract Rectangle CalculateLayoutArea(UIElement child);

        /// <summary>
        /// Gets or sets a value indicating whether this container requires that a scissor
        /// rectangle be applied prior to rendering its children.
        /// </summary>
        protected Boolean RequiresScissorRectangle
        {
            get { return requiresScissorRectangle; }
            set { requiresScissorRectangle = value; }
        }

        /// <inheritdoc/>
        internal override void Draw(UltravioletTime time, SpriteBatch spriteBatch)
        {
            if (View == null || !Visible)
                return;

            base.Draw(time, spriteBatch);

            var scissor     = RequiresScissorRectangle;
            var scissorRect = default(Rectangle?);

            if (scissor)
            {
                ApplyScissorRectangle(spriteBatch, out scissorRect);
            }

            foreach (var child in children)
            {
                if (child.Visible)
                {
                    child.Draw(time, spriteBatch);
                }
            }

            if (scissor)
            {
                RestoreScissorRectangle(spriteBatch, scissorRect);
            }
        }

        /// <inheritdoc/>
        internal override void Update(UltravioletTime time)
        {
            base.Update(time);

            foreach (var child in children)
            {
                child.Update(time);
            }

            if (layoutRequested)
            {
                layoutRequested = false;
                PerformLayout();
            }
        }

        /// <inheritdoc/>
        internal override void UpdateViewModel(Object viewModel)
        {
            base.UpdateViewModel(viewModel);

            foreach (var child in children)
            {
                child.UpdateViewModel(viewModel);
            }
        }

        /// <inheritdoc/>
        internal override void UpdateView(UIView view)
        {
            base.UpdateView(view);

            foreach (var child in children)
            {
                child.UpdateView(view);
            }
        }

        /// <summary>
        /// Gets the element at the specified point in element space.
        /// </summary>
        /// <param name="x">The x-coordinate of the point to evaluate.</param>
        /// <param name="y">The y-coordinate of the point to evaluate.</param>
        /// <returns>The element at the specified point in element space, or null if no such element exists.</returns>
        internal override UIElement GetElementAtPointInternal(Int32 x, Int32 y)
        {
            foreach (var child in children)
            {
                var element = child.GetElementAtPointInternal(x - child.ContainerRelativeX, y - child.ContainerRelativeY);
                if (element != null)
                {
                    return element;
                }
            }
            return base.GetElementAtPointInternal(x, y);
        }

        /// <summary>
        /// Updates the container's absolute screen position.
        /// </summary>
        /// <param name="x">The x-coordinate of the container's absolute screen position.</param>
        /// <param name="y">The y-coordinate of the container's absolute screen position.</param>
        internal void UpdateAbsoluteScreenPosition(Int32 x, Int32 y)
        {
            this.absoluteScreenX = x;
            this.absoluteScreenY = y;

            foreach (var child in children)
            {
                var container = child as UIContainer;
                if (container != null)
                {
                    container.UpdateAbsoluteScreenPosition(x + container.ContainerRelativeX, y + container.ContainerRelativeY);
                }
            }
        }

        /// <summary>
        /// Gets the x-coordinate of the element's absolute screen position.
        /// </summary>
        internal override Int32 AbsoluteScreenXInternal
        {
            get { return absoluteScreenX; }
        }

        /// <summary>
        /// Gets the y-coordinate of the element's absolute screen position.
        /// </summary>
        internal override Int32 AbsoluteScreenYInternal
        {
            get { return absoluteScreenY; }
        }

        /// <summary>
        /// Immediately recalculates the layout of the specified child element.
        /// </summary>
        /// <param name="child">The child element for which to calculate a layout.</param>
        /// <param name="single">A value indicating whether this is the only element being laid out.</param>
        private void PerformLayoutInternal(UIElement child, Boolean single)
        {
            var layout = CalculateLayoutArea(child);
            child.ContainerRelativeLayout = layout;

            var container = child as UIContainer;
            if (container != null)
            {
                container.PerformLayout();
                container.UpdateAbsoluteScreenPosition(
                    absoluteScreenX + container.ContainerRelativeX, 
                    absoluteScreenY + container.ContainerRelativeY);
            }

            if (single)
                DetermineIfScissorRectangleIsRequired();
        }

        /// <summary>
        /// Determines whether a scissor rectangle must be applied to this container.
        /// </summary>
        private void DetermineIfScissorRectangleIsRequired()
        {
            var required = false;
            foreach (var child in children)
            {
                if (child.ContainerRelativeX < 0 || 
                    child.ContainerRelativeY < 0 ||
                    child.ContainerRelativeX + child.CalculatedWidth > CalculatedWidth ||
                    child.ContainerRelativeY + child.CalculatedHeight > CalculatedHeight)
                {
                    required = true;
                    break;
                }
            }
            RequiresScissorRectangle = required;
        }

        /// <summary>
        /// Applies the container's scissor rectangle to the graphics device.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch with which the container is being rendered.</param>
        /// <param name="previous">The previous scissor rectangle, or <c>null</c> if the scissor test is disabled.</param>
        private void ApplyScissorRectangle(SpriteBatch spriteBatch, out Rectangle? previous)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, Graphics.BlendState.AlphaBlend);

            var scissorRectContainer = Ultraviolet.GetGraphics().GetScissorRectangle() ?? View.Area;
            var scissorRectElement   = new Rectangle(AbsoluteScreenX, AbsoluteScreenY, CalculatedWidth, CalculatedHeight);
            var scissorRectIntersect = default(Rectangle);
            Rectangle.Intersect(ref scissorRectContainer, ref scissorRectElement, out scissorRectIntersect);

            Ultraviolet.GetGraphics().SetScissorRectangle(scissorRectIntersect);

            previous = scissorRectContainer;
        }

        /// <summary>
        /// Restores the previous scissor rectangle after the container is done rendering its children.
        /// </summary>
        /// <param name="spriteBatch">The sprite batch with which the container is being rendered.</param>
        /// <param name="rect">The scissor rectangle to apply to the graphics device.</param>
        private void RestoreScissorRectangle(SpriteBatch spriteBatch, Rectangle? rect)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            Ultraviolet.GetGraphics().SetScissorRectangle(rect);
        }

        // Property values.
        private readonly UIElementCollection children;
        private Int32 absoluteScreenX;
        private Int32 absoluteScreenY;
        private Boolean requiresScissorRectangle;

        // State values.
        private Boolean layoutRequested;
    }
}
