﻿using Ultraviolet.Audio;
using Ultraviolet.Core;
using Ultraviolet.FMOD.Audio;

namespace Ultraviolet.FMOD
{
    /// <summary>
    /// Initializes factory methods for the FMOD implementation of the audio subsystem.
    /// </summary>
    [Preserve(AllMembers = true)]
    public sealed class FMODUltravioletFactoryInitializer : IUltravioletFactoryInitializer
    {
        /// <inheritdoc/>
        public void Initialize(UltravioletContext owner, UltravioletFactory factory)
        {
            factory.SetFactoryMethod<SongPlayerFactory>((uv) => new FMODSongPlayer(uv));
            factory.SetFactoryMethod<SoundEffectPlayerFactory>((uv) => new FMODSoundEffectPlayer(uv));
        }
    }
}