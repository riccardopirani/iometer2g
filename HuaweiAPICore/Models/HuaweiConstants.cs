using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class HuaweiConstants
    {

        public static class DeviceTypeEnum
        {
            public const int STRING_INVERTER = 1;
            public const int DAQ = 2;
            public const int STRING = 3;
            
            public const int BAY = 6;
            public const int BUS = 7;
            public const int BOX_TYPE_SUBSTATION = 8;
            public const int ELECTRIC_METER_OF_BOX_TYPE_SUBSTATION = 9;
            public const int ENVIRONMENTAL_MONITOR = 10;
            public const int AC_COMBINER_BOX = 11;
            public const int NORTHBOUND = 12;
            public const int DATA_PROCESSING_UNIT = 13;
            public const int CENTRALIZED_INVERTER = 14;
            public const int DC_COMBINER_BOX = 15;
            public const int GENERAL_EQUIPMENT = 16;
            public const int GATEWAY_METER = 17;
            public const int ELECTRIC_METER_FOR_LINE_AT_COLLECTION_STATION = 18;
            public const int ELECTRIC_METER_IN_PRODUCTION_AREA_FOR_FACTORY_POWER = 19;
            public const int PHOTOVOLTAIC_POWER_PREDICTION_SYSTEM = 20;
            public const int ELECTRIC_METER_IN_NON_PRODUCTION_AREA_FOR_FACTORY_POWER = 21;
            public const int PID = 22;
            public const int VIRTUAL_EQUIPMENT_OF_POWER_PLANT_SUPERVISORY_SYSTEM = 23;
            public const int POWER_QUALITY_DEVICE = 24;
            public const int STEP_UP_SUBSTATION = 25;
            public const int PHOTOVOLTAIC_GRID_CO_NNECTED_CABINET = 26;
            public const int PHOTOVOLTAIC_GRID_CONNECTED_SCREEN = 27;
            public const int PINNET_DAQ = 37;
            public const int HOUSEHOLD_INVERTER = 38;
            public const int ENERGY_STORAGE = 39;
            public const int HOUSEHOLD_ELECTRIC_METER = 40;
        }

        public class FailCodeEnum
        {

            public const int NotExixted = 20001;  //Nonexistent number of third party
            public const int Forbidden = 20002;  //Third party forbidden
            public const int TimeOut = 20003;  //Time-out of third party system
            public const int AbnormalServer = 20004;  //Abnormal server
            public const int EquipmentIDEmpty = 20005;  //The equipment ID cannot be empty in the input parameter
            public const int EquipmentIDNotMatchingType = 20006;  //There is the equipment that does not match the equipment type in the incoming equipment.


            public const int Need_to_login = 305;       //It is not currently logged in and needs to be logged in again.
            public const int NoElevatePrivileges = 401;       //There is no relevant data interface privilege
            public const int InterfaceFrequentlyAccessed = 407;       //The interface is frequently accessed

        }

    }
}
