using System;
using System.Collections.Generic;
using System.Linq;

namespace EdifToSystemC
{
    static class Convertation
    {
        //список блоков(модулей), представленных на логической схеме
        public static List<Module> Modules = new List<Module>();


        /// <summary>
        /// Анализирует EDIF файл и добавляет объекты класса Module в список "Modules"
        /// </summary>
        /// <param name="edif">строковое представление EDIF файла</param>
        /// <returns>false, если не найдено ни одного модуля (блока)</returns>
        public static bool AnalyseEDIF(String edif) {
            int CP = 0,     //Char Pointer - указатель на текущий обрабатываемый символ строки edif, 
                CPnext = 0, //CPnext - указывает на следующий блок (модуль)
                CPcur = 0; //CPcur - указывает на начало текущего блока (модуля)

            CP = edif.IndexOf("cell "); //поиск первого модуля
            if (CP == -1) return false; //если не найден ни один модуль - вернуть false

            while (CP != edif.Length){ //пока не найдены все модули
                CPcur = CP;
                CP += 5;
                CPnext = edif.IndexOf("cell ",CP);
                if (CPnext == -1) CPnext = edif.Length;

                Modules.Add(new Module(edif.Substring(CP, edif.IndexOf(" ", CP) - CP)));//Добавление нового модуля в список Modules

                //создание списка портов для текущего модуля
                CP = edif.IndexOf("port ", CP);
                if (CP == -1) CP = edif.Length;
                while (CP < CPnext){
                    CP += 5;
                    Modules.Last().Ports.Add(new Port(edif.Substring(CP, edif.IndexOf(" ", CP) - CP), GetPortDirection(edif,CP)));
                    CP = edif.IndexOf("port ", CP);
                    if (CP == -1) CP = edif.Length;
                }

                //создание списка внутренних модулей для текущего модуля
                CP = CPcur;
                CP = edif.IndexOf("instance ", CP);
                if (CP == -1) CP = edif.Length;
                while (CP < CPnext)
                {
                    CP += 9;
                    Modules.Last().Modules.Add( new ModuleInstance(edif.Substring(CP, edif.IndexOf(" ", CP) - CP),
                                                Modules.Find(x => x.Name == GetModuleRef(edif,CP)) ));
                    CP = edif.IndexOf("instance ", CP);
                    if (CP == -1) CP = edif.Length;
                }

                //создание списка внутренних соединений для текущего модуля --(не закончено)--
                CP = CPcur;
                CP = edif.IndexOf("(net ", CP);
                if (CP == -1) CP = edif.Length;
                while (CP < CPnext)
                {
                    CP += 5;
                    Modules.Last().Signals.Add(new Signal(edif.Substring(CP, edif.IndexOf(" ", CP) - CP)));

                    //создание списка подключений (порт, экземпляр модуля) для текущего соединения
                    int CPnextSignal = edif.IndexOf("(net ", CP);
                    if (CPnextSignal == -1) CPnextSignal = edif.Length;
                    CP = edif.IndexOf("portRef ", CP);
                    if (CP == -1) CP = edif.Length;

                    while (CP < CPnextSignal){
                        CP += 8;
                        ModuleInstance moduleIns = Modules.Last().Modules.Find(x => x.InstanceName == GetInstanceRef(edif, CP));
                        if (moduleIns != null) Modules.Last().Signals.Last().PortsOfModules.Add(moduleIns,
                                            moduleIns.ModuleRef.Ports.Find(x => x.Name == edif.Substring(CP, edif.IndexOf(" ", CP) - CP)));
                        else Modules.Last().Signals.Last().PortsOfModules.Add(new ModuleInstance("MainModule", Modules.Last()),
                                            Modules.Last().Ports.Find(x => x.Name == edif.Substring(CP, edif.IndexOf(" ", CP) - CP)));
                        CP = edif.IndexOf("portRef ", CP);
                        if (CP == -1) CP = edif.Length;
                    }

                    CP = CPnextSignal;
                }

                CP = CPnext;
            }
            return true;
        }


        /// <summary>
        /// Возвращает строку с описанием схемы на SystemC
        /// </summary>
        /// <returns></returns>
        public static String GetSystemCCode(){
            String output = "#include \"systemc.h\"\r\n";
            bool typeDeclared = false; //true, когда уже указан тип данных (для перечисления портов/сигналов/модулей)

            foreach (Module module in Modules) {
                output += "SC_MODULE(" + module.Name + ") {\r\n";

                //объявление sc_in, sc_out, sc_inout
                for (int type = 0; type < 3; type++){
                    foreach (Port port in module.Ports){
                        if (port.Type == type){
                            if (!typeDeclared){//если еще не добавлен тип порта
                                switch (type){
                                    case 0: output += "\tsc_in<bool>"; break;
                                    case 1: output += "\tsc_out<bool>"; break;
                                    case 2: output += "\tsc_inout<bool>"; break;
                                }
                                typeDeclared = true;
                            }
                            output += " " + port.Name + ",";
                        }
                    }
                    if (output[output.Length - 1] == ',') output = output.Remove(output.Length - 1) + ";\r\n";
                    typeDeclared = false;
                }

                //объявление экземпляров модулей
                foreach (Module curModule in Modules){
                    foreach (ModuleInstance moduleIns in module.Modules){
                        //если тип экземпляра модуля совпадает с типом (curModule), который объявляется в данный момент, то добавить экземпляр к списку
                        if (moduleIns.ModuleRef == curModule){
                            if (!typeDeclared)
                            {//если еще не добавлен тип модуля
                                output += "\t" + curModule.Name;
                                typeDeclared = true;
                            }
                            output += " " + moduleIns.InstanceName + ",";
                        }
                    }
                    if (output[output.Length - 1] == ',') output = output.Remove(output.Length - 1) + ";\r\n";
                    typeDeclared = false;
                }

                //объявление внутренних сигналов
                foreach (Signal curSignal in module.Signals){
                    //если для данного соединения нет одноименного порта, 
                    //значит необходимо объявить это соединение как внутреннее
                    if (module.Ports.Find(x => x.Name == curSignal.Name) == null) {
                        if (!typeDeclared)
                        {//если еще не добавлен тип данных (sc_signal<bool>)
                            output += "\t" + "sc_signal<bool>";
                            typeDeclared = true;
                        }
                        output += " " + curSignal.Name + ",";
                    }
                }
                if (output[output.Length - 1] == ',') output = output.Remove(output.Length - 1) + ";\r\n";
                typeDeclared = false;
                output += "\r\n";

                //добавление шаблона для описания поведенческой модели, если нет внутренних модулей
                if (module.Modules.Count == 0) {
                    output += "\tvoid do_" + module.Name + "() {\r\n"
                    + "\t\t//---- Add module behavour description here " + module.Name + " ----\r\n\t}\r\n\r\n"
                    + "\tSC_CTOR(" + module.Name + ") {\r\n"
                    + "\t\tSC_METHOD(do_" + module.Name + ");\r\n"
                    + "\t\tsensitive";
                    foreach (Port curPort in module.Ports) {
                        if (curPort.Type == 0) //если входной порт
                            output += " << " + curPort.Name;
                    }
                    output += ";\r\n\t}\r\n";
                }
                
                //или списка соединений(если для данного модуля указаны соединения)
                else {
                    output += "\tSC_CTOR(" + module.Name + "):";

                    foreach (ModuleInstance moduleIns in module.Modules) {
                        output += " " + moduleIns.InstanceName + "(\"" + moduleIns.InstanceName + "_lbl\"),";
                    }
                    if (output[output.Length - 1] == ',') output = output.Remove(output.Length - 1) + " {";

                    foreach (Signal curSignal in module.Signals) {
                        output += "\r\n\t\t";
                        foreach (ModuleInstance curInstance in curSignal.PortsOfModules.Keys){
                            if (curInstance.InstanceName != "MainModule")
                                output += curInstance.InstanceName + "." + curSignal.PortsOfModules[curInstance].Name 
                                    + "(" + curSignal.Name + "); ";
                        }
                    }

                    output += "\r\n\t}\r\n";
                }
                output += "};\r\n\r\n";
            }

            return output;
        }


        /// <summary>
        /// Возвращает основную информацию о логической схеме
        /// </summary>
        /// <returns></returns>
        public static String GetSchemeInfo() {
            if (Modules.Count == 0) return "";
            String info = "Modules count: " + Convert.ToString(Modules.Count) 
                + ".\r\nModules:\r\n";
            foreach (Module module in Modules) {
                info += "  " + module.Name + ": ";
                if (module.Ports.Count > 0) info += "ports count: " + Convert.ToString(module.Ports.Count) + ", ";
                if (module.Modules.Count > 0) info += "module instance count: " + Convert.ToString(module.Modules.Count) + ", ";
                if (module.Signals.Count > 0) info += "signals count: " + Convert.ToString(module.Signals.Count) + ", ";
                info = info.Remove(info.Length - 2) + ";\r\n";
            }
            info = info.Remove(info.Length - 3) + ".";
            return info;
        }



        /// <summary>
        /// Возвращает кодовое значение типа(направления) порта 
        /// после анализа строки edif начиная со значения pointer.
        /// 0-INPUT, 1-OUTPUT, 2-INOUT
        /// </summary>
        private static byte GetPortDirection(String edif, int pointer) {
            pointer = edif.IndexOf("direction ", pointer);
            if (pointer == -1) throw new ArgumentException("Undefined port type.");
            pointer += 10;
            String PortDirection = edif.Substring(pointer, edif.IndexOf(")", pointer) - pointer);

            if (PortDirection == "INPUT") return 0;
            else if (PortDirection == "OUTPUT") return 1;
            else if (PortDirection == "INOUT") return 2;
            else throw new ArgumentException("Undefined port type.");
        }

        /// <summary>
        /// Возвращает название модуля на который ссылается экземпляр модуля
        /// после анализа строки edif начиная со значения pointer.
        /// </summary>
        private static String GetModuleRef(String edif, int pointer) {
            pointer = edif.IndexOf("cellRef ", pointer);
            if (pointer == -1) throw new ArgumentException("Module reference not found.");
            pointer += 8;
            return edif.Substring(pointer, edif.IndexOf(" ", pointer) - pointer);
        }

        /// <summary>
        /// Возвращает название модуля на который ссылается экземпляр модуля
        /// после анализа строки edif начиная со значения pointer.
        /// </summary>
        private static String GetInstanceRef(String edif, int pointer) {
            int localpointer = edif.IndexOf("instanceRef ", pointer);
            if (localpointer == -1) localpointer = edif.Length;

            //если instanceRef не указана для текущего portRef, вернуть пустую строку
            if (localpointer > edif.IndexOf(")", pointer)) return "";
            localpointer += 12;
            return edif.Substring(localpointer, edif.IndexOf(")", localpointer) - localpointer);
        }
    }
}
