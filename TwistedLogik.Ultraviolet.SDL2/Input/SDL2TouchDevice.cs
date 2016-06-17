﻿using System;
using System.Collections.Generic;
using TwistedLogik.Nucleus;
using TwistedLogik.Nucleus.Messages;
using TwistedLogik.Ultraviolet.Input;
using TwistedLogik.Ultraviolet.SDL2.Messages;
using TwistedLogik.Ultraviolet.SDL2.Native;

namespace TwistedLogik.Ultraviolet.SDL2.Input
{
    /// <summary>
    /// Represents the SDL2 implementation of the <see cref="TouchDevice"/> class.
    /// </summary>
    public sealed class SDL2TouchDevice : TouchDevice,
        IMessageSubscriber<UltravioletMessageID>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SDL2TouchDevice"/> class.
        /// </summary>
        /// <param name="uv">The Ultraviolet context.</param>
        /// <param name="index">The index of the SDL2 touch device represented by this object.</param>
        public SDL2TouchDevice(UltravioletContext uv, Int32 index)
            : base(uv)
        {
            var id = SDL.GetTouchDevice(index);
            if (id == 0)
                throw new SDL2Exception();

            this.sdlTouchID = id;

            uv.Messages.Subscribe(this,
                SDL2UltravioletMessages.SDLEvent);
        }

        /// <inheritdoc/>
        void IMessageSubscriber<UltravioletMessageID>.ReceiveMessage(UltravioletMessageID type, MessageData data)
        {
            if (type != SDL2UltravioletMessages.SDLEvent)
                return;

            var evt = ((SDL2EventMessageData)data).Event;
            switch (evt.type)
            {
                case SDL_EventType.FINGERDOWN:
                    {
                        if (evt.tfinger.touchId != sdlTouchID)
                            return;					

                        BeginTap(evt.tfinger.fingerId, evt.tfinger.x, evt.tfinger.y);
                        OnFingerDown(evt.tfinger.fingerId, evt.tfinger.x, evt.tfinger.y, evt.tfinger.pressure);
                    }
                    break;

                case SDL_EventType.FINGERUP:
                    {
                        if (evt.tfinger.touchId != sdlTouchID)
                            return;

						var tapIndex = default(Int32?);
						EndTap(evt.tfinger.fingerId, evt.tfinger.x, evt.tfinger.y, out tapIndex);
                        OnFingerUp(evt.tfinger.fingerId, evt.tfinger.x, evt.tfinger.y, evt.tfinger.pressure);

						if (tapIndex.HasValue)
							RemoveTap(tapIndex.Value);
                    }
                    break;

                case SDL_EventType.FINGERMOTION:
                    {
                        if (evt.tfinger.touchId != sdlTouchID)
                            return;

                        OnFingerMotion(evt.tfinger.fingerId, evt.tfinger.x, evt.tfinger.y, evt.tfinger.dx, evt.tfinger.dy, evt.tfinger.pressure);
                    }
                    break;

                case SDL_EventType.MULTIGESTURE:
                    {
                        if (evt.mgesture.touchId != sdlTouchID)
                            return;

                        OnMultiTouchGesture(evt.mgesture.x, evt.mgesture.y, evt.mgesture.dTheta, evt.mgesture.dDist, evt.mgesture.numFingers);
                    }
                    break;
            }
        }

        /// <summary>
        /// Resets the device's state in preparation for the next frame.
        /// </summary>
        public void ResetDeviceState()
        {
            Contract.EnsureNotDisposed(this, Disposed);

            this.tapsLastFrame.Clear();
        }

        /// <inheritdoc/>
        public override void Update(UltravioletTime time)
        {
            Contract.EnsureNotDisposed(this, Disposed);

            this.timestamp = time.TotalTime.TotalMilliseconds;
        }

        /// <inheritdoc/>
        public override Boolean WasTapped()
        {
            return this.tapsLastFrame.Count > 0;
        }

        /// <inheritdoc/>
        public override Boolean WasTapped(RectangleF area)
        {
            foreach (var tap in tapsLastFrame)
            {
                if (area.Contains(tap.X, tap.Y))
                {
                    return true;
                }
            }
            return false;
        }

		/// <inheritdoc/>
		public override bool WasTappedBy(int index)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override Boolean GetActiveTouch(Int32 index, out TouchInfo info)
		{
			unsafe
			{	
				var finger = SDL.GetTouchFinger(sdlTouchID, index);
				if (finger == null)
				{
					info = default(TouchInfo);
					return false;
				}

				info = new TouchInfo(finger->id, finger->x, finger->y, finger->pressure);
				return true;
			}
		}

		/// <inheritdoc/>
		public override bool WasTappedBy(int index, RectangleF area)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public override Int64? GetFingerIDFromIndex(Int32 index)
		{
			for (int i = 0; i < tapsInProgress.Count; i++)
			{
				if (tapsInProgress[i].FingerIndex == index)
					return tapsInProgress[i].FingerID;
			}
			return null;
		}

		/// <inheritdoc/>
		public override Int32? GetIndexFromFingerID(Int64 fingerID)
		{
			for (int i = 0; i < tapsInProgress.Count; i++)
			{
				if (tapsInProgress[i].FingerID == fingerID)
					return tapsInProgress[i].FingerIndex;
			}
			return null;
		}

        /// <inheritdoc/>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (!Ultraviolet.Disposed)
                {
                    Ultraviolet.Messages.Unsubscribe(this);
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Begins a tap event.
        /// </summary>
        /// <param name="fingerID">The identifier of the finger that was pressed.</param>
        /// <param name="x">The x-coordinate at which the finger was pressed.</param>
        /// <param name="y">The y-coordinate at which the finger was pressed.</param>
        private void BeginTap(Int64 fingerID, Single x, Single y)
        {
			var fingerIndex = tapsInProgress.Count;
			var fingerData = new TouchTapData(fingerID, fingerIndex, x, y, timestamp);
			tapsInProgress.Add(fingerData);
        }

        /// <summary>
        /// Ends a tap event.
        /// </summary>
        /// <param name="fingerID">The identifier of the finger that was released.</param>
        /// <param name="x">The x-coordinate at which the finger was released.</param>
        /// <param name="y">The y-coordinate at which the finger was released.</param>
		/// <param name="index">The index of the tap that was ended.</param>
		private void EndTap(Int64 fingerID, Single x, Single y, out Int32? index)
        {
			index = GetIndexFromFingerID(fingerID);
			if (index == null)
				return;

			var data = tapsInProgress[index.Value];

            var dx = Math.Abs(x - data.X);
            if (dx > MaximumTapDistance)
                return;

            var dy = Math.Abs(y - data.Y);
            if (dy > MaximumTapDistance)
                return;
            
            var dt = timestamp - data.Timestamp;
            if (dt > MaximumTapDelay)
                return;
			
            OnTap(fingerID, data.X, data.Y);
        }

		/// <summary>
		/// Removes a tap event from the device's list.
		/// </summary>
		/// <param name="index">The index of the tap event to remove.</param>
		private void RemoveTap(Int32 index)
		{
			var tap = tapsInProgress[index];
			tapsInProgress.RemoveAt(index);
			tapsLastFrame.Add(tap);
		}

        // State values.
        private readonly Int64 sdlTouchID;
        private Double timestamp;

        // Data for all outstanding tap events.
        private readonly List<TouchTapData> tapsLastFrame = new List<TouchTapData>(5);
        private readonly List<TouchTapData> tapsInProgress = new List<TouchTapData>(5);
    }
}
