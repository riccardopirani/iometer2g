using EIEntityCore.Models.Huawei;
using EILib.Dao.Managers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HuaweiAPICore.Managers
{
    public class FormulaResolver
    {


        public bool ResolveAllFormulas(Dictionary<string, object> DatiInArrivo, List<HuaweiAPIDimension> parameters)
        {
            bool fl = true;

            try
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    string result = "";
                    if (ResolveFormula(DatiInArrivo, parameters[i], ref result))
                    {
                        Console.WriteLine($"  Processo {parameters[i].DIMENSION} '{parameters[i].FORMULA}'  valore calcolato '{result}'");
                    }
                    else
                    {
                        Console.WriteLine($"  -- Processo {parameters[i].DIMENSION} '{parameters[i].FORMULA}'  Errore nella formula!");
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message}", ce);
                fl = false;
            }
            return fl;
        }


        List<string> EstraiVariabiliDaFormula(string formula)
        {
            List<string> vars = new List<string>();
            string curVar = "";
            if (!string.IsNullOrEmpty(formula))
            {
                bool startFounded = false;
                for (int i = 0; i < formula.Length; i++)
                {
                    if (formula[i] == '#')
                    {
                        if (startFounded)
                        {
                            vars.Add(curVar);
                            startFounded = false;
                        }
                        else if (!startFounded)
                        {
                            curVar = "";
                            startFounded = true;
                        }
                    }
                    else
                    {
                        if (startFounded)
                        {
                            curVar += formula[i];
                        }
                    }
                }
            }
            return vars;
        }


        //risolve la formula passata utilizzando le datatable
        //nb:la formula non deve contenere variabili, tutte le var devono essere preventivamente sostituite coi valori
        bool ResolveFormulaDataTable(HuaweiAPIDimension parameter, string formula, ref double? result)
        {
            bool fl = true;
            /*
             * string soluz = "(2+3)*2-5";
               string soluz2 = "12+(2+3)*2-5-(6*3)";
               Resolveformula(soluz, ref res);
               Resolveformula(soluz2, ref res);
             */

            try
            {
                if (!string.IsNullOrEmpty(formula))
                {
                    if (formula.Contains("-nan"))
                    {
                        fl = false;
                    }
                    else
                    {
                        object res = new System.Data.DataTable().Compute(formula, null);
                        if (res != null)
                            result = Convert.ToDouble(res);
                        else
                        {
                            LoggerBase.MyLogger.Error($"Impossibile calcolare il risultato della formula '{formula}' DIMENSION '{parameter.DIMENSION}'");
                            fl = false;
                        }
                    }


                }
            }
            catch (Exception )
            {
                //LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message} - Impossibile calcolare il risultato della formula '{formula}'  DIMENSION '{parameter.DIMENSION}'", ce);
                fl = false;
            }
            return fl;
        }


        public bool ResolveFormula(Dictionary<string, object> DatiInArrivo, HuaweiAPIDimension parameter, ref string result)
        {
            bool fl = true;
            string curMember = "";
            try
            {
                string subsParam = "";
                double? res = null;
                //string soluz = "(2+3)*2-5";
                //string soluz2 = "12+(2+3)*2-5-(6*3)";
                //Resolveformula(soluz, ref res);
                //Resolveformula(soluz2, ref res);
                bool singleVar = false;
                bool containOperator = false;


                if (!string.IsNullOrEmpty(parameter.FORMULA))
                {
                    // (#JP38#/2)+(#JP39#/2)

                    curMember = "";
                    //decimal? curResult = null;
                    //bool capturingMember = false;
                    for (int i = 0; i < parameter.FORMULA.Length; i++)
                    {
                        char pp = parameter.FORMULA[i];
                        //if (IsVariable(pp))
                        //{
                        //    if (capturingMember == false)
                        //        capturingMember = true;
                        //    else
                        //        capturingMember = false;
                        //}
                        if (IsOperator(pp))
                        {
                            containOperator = true;
                            if (curMember.Contains("#"))
                            {
                                string noSharp = curMember.Replace("#", "");
                                if (DatiInArrivo.ContainsKey(noSharp))
                                {
                                    object varValue = DatiInArrivo[noSharp];
                                    if (varValue != null)
                                    {
                                        //subsParam += varValue.ToString();
                                        if (varValue.GetType() == typeof(double))
                                        {
                                            double? spVal = (double)varValue;
                                            subsParam += spVal.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                                        }
                                        else if (varValue.GetType() == typeof(decimal))
                                        {
                                            decimal? spVal = (decimal)varValue;
                                            subsParam += spVal.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                                        }
                                        else
                                            subsParam += varValue.ToString();
                                    }
                                }
                                else
                                {
                                    EILib.Dao.Managers.LoggerBase.MyLogger.Error($"NEl Json non esiste il parametro relativo alla formula '{noSharp}'  per OUT '{parameter.NAME}' DIMENSION {parameter.DIMENSION}");
                                }
                            }
                            else
                            {
                                subsParam += curMember;
                            }
                            subsParam += pp;
                            curMember = "";
                        }
                        else
                        {
                            //if ((!IsVariable(pp)) && (!IsOperator(pp)))
                            if (!IsOperator(pp))
                            {
                                curMember += pp;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(curMember))
                    {
                        if (curMember.Contains("#"))
                        {
                            singleVar = true;
                            //qui entro nel caso in cui abbia una variabile secca tipo #IDDEVICE#
                            string noSharp = curMember.Replace("#", "");
                            if (DatiInArrivo.ContainsKey(noSharp))
                            {
                                object varValue = DatiInArrivo[noSharp];
                                if (varValue != null)
                                {
                                    if (varValue.GetType() == typeof(double))
                                    {
                                        double? spVal = (double)varValue;
                                        subsParam += spVal.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                                    }
                                    else if (varValue.GetType() == typeof(decimal))
                                    {
                                        decimal? spVal = (decimal)varValue;
                                        subsParam += spVal.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                                    }
                                    else
                                        subsParam += varValue.ToString();
                                }
                            }
                        }
                        else
                        {
                            //altri valori rimasti in coda
                            subsParam += curMember;
                        }
                    }
                }

                if (subsParam.Contains("#"))
                {
                    throw new Exception($"Impossibile risolvere la formula, ci sono variabili, trovate variabili non sostituite! subsParam'{subsParam}'");
                }



                if (!singleVar)
                {

                    parameter.util_isCalculated = false;
                    if ((parameter.DIMENSION == "IDDEVICE")
                        || (parameter.DIMENSION == "PAC_SIGN")
                        || (parameter.DIMENSION == "RUN TIME_SIGN")
                        || (parameter.DIMENSION == "RPAC_SIGN")
                        )

                    {
                        result = parameter.FORMULA;
                        parameter.util_isCalculated = true;
                    }
                    else if (ResolveFormulaDataTable(parameter, subsParam, ref res))
                    {
                        if (res != null)
                            result = res.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                        parameter.util_realValue = result;
                        parameter.util_isCalculated = true;
                    }

                }
                else
                {
                    if (containOperator)
                    {
                        if (ResolveFormulaDataTable(parameter, subsParam, ref res))
                        {
                            if (res != null)
                                result = res.Value.ToString(CultureInfo.CreateSpecificCulture("en-GB"));
                            parameter.util_realValue = result;
                            parameter.util_isCalculated = true;
                        }
                    }
                    else
                    {
                        result = subsParam;
                        parameter.util_isCalculated = true;
                    }
                }
            }
            catch (Exception ce)
            {
                LoggerBase.MyLogger.Error($"Exception in {MethodBase.GetCurrentMethod().Name} : {ce.Message} - curMember '{curMember}'", ce);
                fl = false;
            }
            return fl;
        }

        bool IsVariable(char pp)
        {
            if (pp == '#')
                return true;
            return false;
        }

        bool IsOperator(char pp)
        {
            bool op = false;
            if (pp == '-')
                op = true;
            if (pp == '+')
                op = true;
            if (pp == '*')
                op = true;
            if (pp == '/')
                op = true;
            if (pp == '!')
                op = true;
            if (pp == '%')
                op = true;
            return op;
        }

        bool ContainsOperator(string formula)
        {
            bool op = false;
            if (formula.Contains("-"))
                op = true;
            if (formula.Contains("+"))
                op = true;
            if (formula.Contains("*"))
                op = true;
            if (formula.Contains("/"))
                op = true;
            if (formula.Contains("!"))
                op = true;
            if (formula.Contains("%"))
                op = true;
            return op;
        }


        public void ResetVolatileData(List<HuaweiAPIDimension> parameters)
        {
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    parameters[i].util_realValue = null;
                    parameters[i].util_isCalculated = false;
                }
            }
        }
    }
}
