using System;

namespace EdifToSystemC
{
    class ModuleInstance
    {
        /// <summary>
        /// Имя экземпляра модуля
        /// </summary>
        public String InstanceName;

        /// <summary>
        /// Модуль, к которому относится данный экзмепляр
        /// </summary>
        public Module ModuleRef;

        public ModuleInstance(String name, Module module) {
            this.InstanceName = name;
            this.ModuleRef = module;
        }
    }
}
