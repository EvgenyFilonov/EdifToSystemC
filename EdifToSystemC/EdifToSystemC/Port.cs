using System;

namespace EdifToSystemC
{
    class Port
    {
        /// <summary>
        /// имя порта
        /// </summary>
        public String Name;

        /// <summary>
        /// тип порта:
        /// 0-INPUT
        /// 1-OUTPUT
        /// 2-INOUT
        /// </summary>
        public byte Type;

        public Port(String name, byte type) {
            this.Name = name;
            this.Type = type;
        }

    }
}
