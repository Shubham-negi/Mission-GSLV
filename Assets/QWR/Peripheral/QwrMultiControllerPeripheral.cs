


using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace qwr
{
    /// <summary>
    /// Main entry point for the QwrMultiControllerPeripheral API.
    /// </summary>
    /// <seealso cref="qwr.QwrBasePeripheral" />
    abstract public class QwrMultiControllerPeripheral :QwrBasePeripheral
    {
        public abstract QwrBasePeripheral GetController(string controllerName);

        protected QwrBasePeripheral[] _controllers = null;
        protected string[] _controllerNames = null;
        protected QwrBasePeripheral _mainController = null;
        protected Dictionary<string, QwrBasePeripheral> _connectedControllers = new Dictionary<string, QwrBasePeripheral>();

        void Start()
        {
            if (_controllers == null)
                return;

            for(int i = 0;i < _controllers.Length;++i)
            {
                QwrBasePeripheral controller = _controllers[i];
                controller.EnableModel(true);
            }
        }

        /// <summary>
        /// Called when [enable].
        /// </summary>
        public void OnEnable()
        {
            if (_controllers == null)
                return;

            for(int i = 0;i < _controllers.Length;++i)
            {
                string name = _controllerNames[i];
                QwrBasePeripheral controller = GetController(name);
                if(controller)
                {
                    _controllers[i] = controller;
                    _connectedControllers[name] = controller;
                }
            }
        }

        /// <summary>
        /// Enables the internal objects.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        override public void EnableInternalObjects(bool enabled)
        {
            if (_controllers == null)
                return;

            QwrPointerManager.Instance.ray.SetVisible(true);

            foreach(QwrBasePeripheral controller in _controllers)
            {
                controller.EnableInternalObjects(enabled);
            }
        }

        /// <summary>
        /// Called when [controller disconnected].
        /// </summary>
        /// <param name="controller">The controller.</param>
        virtual public void OnControllerDisconnected(QwrBasePeripheral controller)
        {
            if (controller == null)
                return;

            controller.EnableInternalObjects(false);

            _connectedControllers.Remove(controller.name);

            if (_connectedControllers.Count == 0)
            {
                _mainController = null;
                SendMessageUpwards("OnPeripheralDisconnected", this);
            }
            else
            {
                if(controller == _mainController)
                {
                    QwrBasePeripheral mainController = _connectedControllers.Values.First<QwrBasePeripheral>();
                    _mainController = mainController;
                }
                
            }
        }

        /// <summary>
        /// Called when [controller connected].
        /// </summary>
        /// <param name="controller">The controller.</param>
        virtual public void OnControllerConnected(QwrBasePeripheral controller)
        {
            if (controller == null)
                return;

            controller.EnableModel(true);

            if(_connectedControllers.Count == 0)
            {
                SendMessageUpwards("OnPeripheralConnected", this);
                controller.EnableInternalObjects(true);
                _mainController = controller;
            }

            _connectedControllers[controller.name] = controller;
        }

        public override bool IsActivating()
        {
            for(int i = 0;i < _controllers.Length;++i)
            {
                if (_controllers[i].IsActivating())
                    return true;
            }
            return false;
        }
    }
}