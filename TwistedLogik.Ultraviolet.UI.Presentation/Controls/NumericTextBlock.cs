﻿using System;
using System.ComponentModel;
using TwistedLogik.Ultraviolet.UI.Presentation.Controls.Primitives;

namespace TwistedLogik.Ultraviolet.UI.Presentation.Controls
{
    /// <summary>
    /// Represents a text block that is optimized for displaying numeric values.
    /// </summary>
    [UvmlKnownType]
    [DefaultProperty("Value")]
    public class NumericTextBlock : TextBlockBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericTextBlock"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="name">The element's identifying name within its namescope.</param>
        public NumericTextBlock(UltravioletContext uv, String id)
            : base(uv, id)
        {

        }

        /// <summary>
        /// Gets or sets the label's value.
        /// </summary>
        public Double Value
        {
            get { return GetValue<Double>(ValueProperty); }
            set { SetValue<Double>(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the format string used to format the label's value.
        /// </summary>
        public String Format
        {
            get { return GetValue<String>(FormatProperty); }
            set { SetValue<String>(FormatProperty, value); }
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Value"/> property changes.
        /// </summary>
        public event UpfEventHandler ValueChanged;

        /// <summary>
        /// Occurs when the value of the <see cref="Format"/> property changes.
        /// </summary>
        public event UpfEventHandler FormatChanged;

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(Double), typeof(NumericTextBlock),
            new PropertyMetadata(null, PropertyMetadataOptions.AffectsMeasure, HandleValueChanged));
        
        /// <summary>
        /// Identifies the <see cref="Format"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(String), typeof(NumericTextBlock),
            new PropertyMetadata(null, PropertyMetadataOptions.AffectsMeasure, HandleFormatChanged));

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        protected virtual void OnValueChanged()
        {
            var temp = ValueChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <summary>
        /// Raises the <see cref="FormatChanged"/> event.
        /// </summary>
        protected virtual void OnFormatChanged()
        {
            var temp = FormatChanged;
            if (temp != null)
            {
                temp(this);
            }
        }

        /// <inheritdoc/>
        protected override void DrawOverride(UltravioletTime time, DrawingContext dc)
        {
            var font = Font;
            if (font != null && font.IsLoaded)
            {
                View.Resources.StringFormatter.Reset();
                View.Resources.StringFormatter.AddArgument(Value);
                View.Resources.StringFormatter.Format(Format ?? "{0}", View.Resources.StringBuffer);

                var face     = font.Resource.Value.GetFace(FontStyle);
                var position = (Vector2)Display.PixelsToDips(AbsolutePosition);

                dc.SpriteBatch.DrawString(face, View.Resources.StringBuffer, position, FontColor * dc.Opacity);
            }
            base.DrawOverride(time, dc);
        }

        /// <inheritdoc/>
        protected override Size2D MeasureOverride(Size2D availableSize)
        {
            var font = Font;
            if (font == null || !font.IsLoaded)
                return Size2D.Zero;

            View.Resources.StringFormatter.Reset();
            View.Resources.StringFormatter.AddArgument(Value);
            View.Resources.StringFormatter.Format(Format ?? "{0}", View.Resources.StringBuffer);

            var face = font.Resource.Value.GetFace(FontStyle);
            return face.MeasureString(View.Resources.StringBuffer);
        }

        /// <inheritdoc/>
        protected override Size2D ArrangeOverride(Size2D finalSize, ArrangeOptions options)
        {
            return finalSize;
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Value"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The dependency object that raised the event.</param>
        private static void HandleValueChanged(DependencyObject dobj)
        {
            var label = (NumericTextBlock)dobj;
            label.OnValueChanged();
        }

        /// <summary>
        /// Occurs when the value of the <see cref="Format"/> dependency property changes.
        /// </summary>
        /// <param name="dobj">The dependency object that raised the event.</param>
        private static void HandleFormatChanged(DependencyObject dobj)
        {
            var label = (NumericTextBlock)dobj;
            label.OnFormatChanged();
        }
    }
}
