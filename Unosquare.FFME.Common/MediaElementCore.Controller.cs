﻿namespace Unosquare.FFME
{
    using Core;
    using Commands;
    using Decoding;
    using System;
    using System.Threading.Tasks;

    public partial class MediaElementCore
    {
        #region Internal Members

        /// <summary>
        /// The command queue to be executed in the order they were sent.
        /// </summary>
        internal MediaCommandManager Commands { get; private set; }

        /// <summary>
        /// Represents a real-time time measuring device.
        /// Rendering media should occur as requested by the clock.
        /// </summary>
        internal Clock Clock { get; } = new Clock();

        /// <summary>
        /// The underlying media container that provides access to 
        /// individual media component streams
        /// </summary>
        internal MediaContainer Container { get; set; } = null;

        #endregion

        #region Public API

        /// <summary>
        /// Begins or resumes playback of the currently loaded media.
        /// </summary>
        public void Play() => Commands.Play();

        /// <summary>
        /// Pauses playback of the currently loaded media.
        /// </summary>
        public void Pause() => Commands.Pause();

        /// <summary>
        /// Pauses and rewinds the currently loaded media.
        /// </summary>
        public void Stop() => Commands.Stop();

        /// <summary>
        /// Opens the specified URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The awaitable task</returns>
        /// <exception cref="InvalidOperationException">Source</exception>
        public async Task Open(Uri uri)
        {
            Source = uri;

            // TODO: Calling this multiple times while an operation is in progress breaks the control :(
            // for now let's throw an exception but ideally we want the user NOT to be able to change the value in the first place.
            if (IsOpening)
                throw new InvalidOperationException($"Unable to change {nameof(Source)} to '{uri}' because {nameof(IsOpening)} is currently set to true.");

            if (uri != null)
            {
                await Commands.CloseAsync()
                    .ContinueWith(async (c) =>
                    {
                        await Commands.OpenAsync(uri)
                            .ContinueWith(p =>
                            {
                                if (LoadedBehavior == CoreMediaState.Play || CanPause == false)
                                    Commands.Play();
                            });
                    });
            }
            else
            {
                await Commands.CloseAsync();
            }
        }

        /// <summary>
        /// Closes the currently loaded media.
        /// </summary>
        /// <returns>The awaitable task</returns>
        public async Task Close() => await Commands.CloseAsync();

        /// <summary>
        /// Seeks to the specified position.
        /// </summary>
        /// <param name="position">New position for the player.</param>
        public void Seek(TimeSpan position) => Commands.Seek(position);

        /// <summary>
        /// Sets the specified playback speed ratio.
        /// </summary>
        /// <param name="targetSpeedRatio">New playback speed ratio.</param>
        public void SetSpeedRatio(double targetSpeedRatio) => Commands.SetSpeedRatio(targetSpeedRatio);

        #endregion
    }
}
