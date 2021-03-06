﻿using System;

namespace Ultraviolet.Graphics
{
    /// <summary>
    /// Represents a factory method which constructs instances of the <see cref="BasicEffect"/> class.
    /// </summary>
    /// <param name="uv">The Ultraviolet context.</param>
    /// <returns>The instance of <see cref="BasicEffect"/> that was created.</returns>
    public delegate BasicEffect BasicEffectFactory(UltravioletContext uv);

    /// <summary>
    /// Represents a basic rendering effect.
    /// </summary>
    public abstract class BasicEffect : Effect, IEffectMatrices
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicEffect"/> class.
        /// </summary>
        /// <param name="impl">The <see cref="EffectImplementation"/> that implements this effect.</param>
        protected BasicEffect(EffectImplementation impl)
            : base(impl)
        {
            this.epDiffuseColor = Parameters["DiffuseColor"];
            this.epWorld = Parameters["World"];
            this.epView = Parameters["View"];
            this.epProjection = Parameters["Projection"];
            this.epSrgbColor = Parameters["SrgbColor"];
            this.epTexture = Parameters["Texture"];

            this.epDiffuseColor.SetValue(Color.White);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BasicEffect"/> class.
        /// </summary>
        /// <returns>The instance of <see cref="BasicEffect"/> that was created.</returns>
        public static BasicEffect Create()
        {
            var uv = UltravioletContext.DemandCurrent();
            return uv.GetFactoryMethod<BasicEffectFactory>()(uv);
        }

        /// <summary>
        /// Gets or sets the material's diffuse color.
        /// </summary>
        public Color DiffuseColor
        {
            get => epDiffuseColor.GetValueColor();
            set => epDiffuseColor.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the effect's world matrix.
        /// </summary>
        public Matrix World
        {
            get => epWorld.GetValueMatrix();
            set => epWorld.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the effect's view matrix.
        /// </summary>
        public Matrix View
        {
            get => epView.GetValueMatrix();
            set => epView.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the effect's projection matrix.
        /// </summary>
        public Matrix Projection
        {
            get => epProjection.GetValueMatrix();
            set => epProjection.SetValue(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether vertex colors are enabled for this effect.
        /// </summary>
        public Boolean VertexColorEnabled
        {
            get => vertexColorEnabled;
            set
            {
                if (vertexColorEnabled != value)
                {
                    vertexColorEnabled = value;
                    OnVertexColorEnabledChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether textures are enabled for this effect.
        /// </summary>
        public Boolean TextureEnabled
        {
            get => textureEnabled;
            set
            {
                if (textureEnabled != value)
                {
                    textureEnabled = value;
                    OnTextureEnabledChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the colors used by this effect should be
        /// converted from the sRGB color space to the linear color space in the vertex shader.
        /// </summary>
        public Boolean SrgbColor
        {
            get => epSrgbColor?.GetValueBoolean() ?? false;
            set => epSrgbColor?.SetValue(value);
        }

        /// <summary>
        /// Gets or sets the texture that is applied to geometry rendered by this effect.
        /// </summary>
        public Texture2D Texture
        {
            get => epTexture.GetValueTexture2D();
            set => epTexture.SetValue(value);
        }

        /// <summary>
        /// Occurs when the value of the <see cref="VertexColorEnabled"/> property changes.
        /// </summary>
        protected virtual void OnVertexColorEnabledChanged()
        {

        }

        /// <summary>
        /// Occurs when the value of the <see cref="TextureEnabled"/> property changes.
        /// </summary>
        protected virtual void OnTextureEnabledChanged()
        {

        }

        /// <summary>
        /// Occurs when the value of the <see cref="SrgbColor"/> property changes.
        /// </summary>
        protected virtual void OnSrgbColorChanged()
        {

        }

        // Cached effect parameters.
        private readonly EffectParameter epDiffuseColor;
        private readonly EffectParameter epWorld;
        private readonly EffectParameter epView;
        private readonly EffectParameter epProjection;
        private readonly EffectParameter epSrgbColor;
        private readonly EffectParameter epTexture;

        // Property values.
        private Boolean vertexColorEnabled;
        private Boolean textureEnabled;
    }
}
