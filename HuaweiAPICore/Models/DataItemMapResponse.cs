using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Models
{
    public sealed class DataItemMapResponse
    {
        public decimal? pv1_u { get; set; }
        public decimal? pv2_u { get; set; }
        public decimal? pv3_u { get; set; }
        public decimal? pv4_u { get; set; }
        public decimal? pv5_u { get; set; }
        public decimal? power_factor { get; set; }
        public decimal? pv6_u { get; set; }
        public decimal? pv7_u { get; set; }
        public decimal? pv8_u { get; set; }
        public decimal? inverter_state { get; set; }
        public decimal? open_time { get; set; }
        public decimal? a_i { get; set; }
        public decimal? total_cap { get; set; }
        public decimal? c_i { get; set; }
        public decimal? b_i { get; set; }
        public decimal? mppt_3_cap { get; set; }
        public decimal? a_u { get; set; }
        public decimal? reactive_power { get; set; }
        public decimal? c_u { get; set; }
        public decimal? temperature { get; set; }
        public decimal? bc_u { get; set; }
        public decimal? b_u { get; set; }
        public decimal? elec_freq { get; set; }
        public decimal? mppt_4_cap { get; set; }
        public decimal? efficiency { get; set; }
        public decimal? day_cap { get; set; }
        public decimal? mppt_power { get; set; }
        public decimal? close_time { get; set; }
        public decimal? mppt_1_cap { get; set; }
        public decimal? pv1_i { get; set; }
        public decimal? pv2_i { get; set; }
        public decimal? pv3_i { get; set; }
        public decimal? active_power { get; set; }
        public decimal? pv4_i { get; set; }
        public decimal? mppt_2_cap { get; set; }
        public decimal? pv5_i { get; set; }
        public decimal? ab_u { get; set; }
        public decimal? ca_u { get; set; }
        public decimal? pv6_i { get; set; }
        public decimal? pv7_i { get; set; }
        public decimal? pv8_i { get; set; }


        //dati della sola batteria
        public decimal? max_discharge_power { get; set; }
        public decimal? max_charge_power { get; set; }
        public decimal? battery_soh { get; set; }
        public decimal? busbar_u { get; set; }
        public decimal? discharge_cap { get; set; }
        public decimal? ch_discharge_power { get; set; }
        public decimal? battery_soc { get; set; }
        public decimal? charge_cap { get; set; }

        //dati del multimetro

        public decimal? active_cap { get; set; }
        public decimal? meter_i { get; set; }
        public decimal? reverse_active_cap { get; set; }
        //public decimal? reactive_power { get; set; }
        //public decimal? power_factor { get; set; }
        //public decimal? active_power { get; set; }
        public decimal? meter_u { get; set; }
        public decimal? grid_frequency { get; set; }

    }
}
