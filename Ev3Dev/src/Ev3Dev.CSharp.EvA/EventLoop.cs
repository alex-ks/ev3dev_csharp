﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp.EvA
{
    /// <summary>
    /// Represents application message loop (similar to Win32Api),
    /// which will poll events and perform actions on each iteration
    /// </summary>
    public class EventLoop
    {
        private class ActionsPrioritizer : IComparer<(Action, int)>
        {
            public int Compare((Action, int) x, (Action, int) y) => x.Item2.CompareTo(y.Item2);
        }

        private SortedSet<(Action action, int priority)> _actions =
            new SortedSet<(Action action, int priority)>(new ActionsPrioritizer());

        private List<Func<bool>> _shutdownEvents = new List<Func<bool>>();

        // All the properties are accessed from the loop thread, so there is no need to
        // mainain a concurrent cache.

        private Action _populateCache;
        private Action _clearCache;
        private bool _shouldPopulateCache = false;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="valuesCache"></param>
        /// <param name="populateCache"></param>
        internal EventLoop(Action populateCache, Action clearCache)
        {
            _populateCache = populateCache ?? throw new ArgumentException("Populate cache function must not be null");
            _clearCache = clearCache ?? throw new ArgumentException("Clear cache function must not be null");
        }

        public bool LoadPropertiesLazily
        {
            get => !_shouldPopulateCache;
            set => _shouldPopulateCache = !value;
        }

        /// <summary>
        /// Registers event trigger and its handler.
        /// </summary>
        /// <param name="trigger">
        /// Will be called on each iteration. If trigger returns true,
        /// event loop will call the handler.
        /// </param>
        /// <param name="handler">Will be called if trigger returns true.</param>
        /// <param name="priority">
        /// Indicates how early trigger will be polled during the iteration.
        /// The lower value, the earlier trigger will be polled.
        /// </param>
        public void RegisterEvent(Func<bool> trigger, Action handler, int priority = int.MaxValue)
        {
            _actions.Add((() => { if (trigger()) handler(); }, priority));
        }

        /// <summary>
        /// Registers action.
        /// </summary>
        /// <param name="action">Will be called on each iteration</param>
        /// <param name="priority">
        /// Indicates how early trigger will be polled during the iteration.
        /// The lower value, the earlier trigger will be polled
        /// </param>
        public void RegisterAction(Action action, int priority = int.MinValue)
        {
            _actions.Add((action, priority));
        }

        /// <summary>
        /// Registers event which will cause loop to stop.
        /// </summary>
        /// <param name="sEvent">If true, event loop will stop iterating</param>
        public void RegisterShutdownEvent(Func<bool> sEvent)
        {
            _shutdownEvents.Add(sEvent);
        }

        /// <summary>
        /// Removes all actions and events from loop lists.
        /// </summary>
        public void Reset()
        {
            _shutdownEvents.Clear();
            _actions.Clear();
        }

        /// <summary>
        /// Starts event loop.
        /// </summary>
        /// <param name="millisecondsCooldown">
        /// Defines sleep period between two iterations.
        /// If equals to zero, there will be no sleep.
        /// </param>
        public void Start(int millisecondsCooldown = 0)
        {
            bool shutdown = false;
            var actionsToPerform = _actions.Select(t => t.action);

            while (!shutdown)
            {
                if (_shouldPopulateCache)
                    _populateCache();

                foreach (var needToShutdown in _shutdownEvents)
                {
                    if (needToShutdown())
                    {
                        shutdown = true;
                        return;
                    }
                }

                foreach (var performAction in actionsToPerform)
                {
                    try { performAction(); }
                    catch (LoopInterruptedException) { return; }
                }

                if (millisecondsCooldown != 0)
                    Thread.Sleep(millisecondsCooldown);

                _clearCache();
            }
        }

        /// <summary>
        /// Starts event loop.
        /// </summary>
        /// <param name="cooldown">Defines sleep period between two iterations.</param>
        public void Start(TimeSpan cooldown)
        {
            Start((int)cooldown.TotalMilliseconds);
        }
    }
}
