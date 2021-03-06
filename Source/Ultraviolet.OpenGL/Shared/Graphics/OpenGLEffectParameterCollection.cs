﻿using System.Collections.Generic;
using Ultraviolet.Graphics;

namespace Ultraviolet.OpenGL.Graphics
{
    /// <summary>
    /// Represents the OpenGL implementation of the EffectParameterCollection class.
    /// </summary>
    public sealed class OpenGLEffectParameterCollection : EffectParameterCollection
    {
        /// <summary>
        /// Initializes a new instance of the OpenGLEffectParameterCollection class.
        /// </summary>
        /// <param name="parameters">The set of parameters to add to the collection.</param>
        public OpenGLEffectParameterCollection(IEnumerable<OpenGLEffectParameter> parameters)
            : base(parameters)
        {

        }
    }
}
