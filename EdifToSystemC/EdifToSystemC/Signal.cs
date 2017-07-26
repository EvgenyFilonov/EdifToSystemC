using System;
using System.Collections.Generic;

namespace EdifToSystemC
{
    class Signal
    {
        /// <summary>
        /// имя внутреннего соединения (сигнала)
        /// </summary>
        public String Name;

        /// <summary>
        /// Таблица соответствия (модуль,порт), подключенных к соединению
        /// </summary>
        public Dictionary<ModuleInstance,Port> PortsOfModules = new Dictionary<ModuleInstance, Port>();

        public Signal(String name) {
            this.Name = name;
        }
    }
}
