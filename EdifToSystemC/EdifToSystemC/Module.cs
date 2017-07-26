using System;
using System.Collections.Generic;

namespace EdifToSystemC
{
    class Module
    {
        /// <summary>
        /// имя модуля
        /// </summary>
        public String Name;

        /// <summary>
        /// список портов
        /// </summary>
        public List<Port> Ports = new List<Port>();

        /// <summary>
        /// список внутренних модулей
        /// </summary>
        public List<ModuleInstance> Modules = new List<ModuleInstance>();

        /// <summary>
        /// список внутренних соединений(сигналов)
        /// </summary>
        public List<Signal> Signals = new List<Signal>();

        public Module(String name) {
            this.Name = name;
        }
    }
}
