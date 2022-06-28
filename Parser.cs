using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;

public class Parser : MonoBehaviour {

	//private List<double> numbers;
    private List<Complex> numbers;
	private List<string> operators;
 	private List<int> xSpotsForGraphing;
//	private List<double> fillerDouble;
//	private List<string> fillerString;
	private string finalAnswerInString;
//	private double finalans;
//	private double standByNum;
	private Complex finalans;
	private Complex standByNum;
	private double standByNumDub;
	public string startEquation;
	private string equationsWithParas;
	private string paraString;
	private string toBeParsed;
	private string numberParts = "1234567890.E-&";			//here - is going to represent a negative sign, not a subtraction sign. Also, & is for a complex number ( 4&5 = 4+5i ) this way it sticks together and doesn't mess up order of operations after the paratheses killer
//	private List<string> variableList = new List<string> {"e","h","g","G","π","Ans","VarA","VarB","VarC","VarD","c"};
//	private List<string> variableReplaceMetricList = new List<string> {


	private int numberOfEndPara;
	private int numberOfBeginPara;
	private int firstEndPara;
	private int prevBeginPara;
	private int varReplaceLoopSaver;
	private bool outsiderUse;

//	private bool inRads = true;
	private string errorReport;

	// Use this for initialization
	void Start () {
        
	}

	public string EquationSolver(string startEquation) {
		errorReport = "";
//		numbers = new List<double> {};
		numbers = new List<Complex> {};
		operators = new List<string> {};
		outsiderUse = false;
        startEquation = startEquation.Replace("‑","-"); // replaces nonbreak hyphen with normal one
        //to allow more multiplication flexiblity
		startEquation = MultiplicationFlex(startEquation);

        equationsWithParas = startEquation;
		varReplaceLoopSaver = 0;
		
		if (equationsWithParas.Contains("</i>") == true) {			//done to avoid unnecessary variable searches
			bool goodVarsInEqn = VariableReplacerValidater(equationsWithParas);
			if (goodVarsInEqn==true) {
				equationsWithParas = VariableReplacer(equationsWithParas);
                Debug.Log("after var replace: "+equationsWithParas+", error?: "+errorReport);
			} else {
				if (varReplaceLoopSaver > 999) {						//for now, this is set to a really high number. Maybe think of a beter way to treat this
					if (!errorReport.Contains("Error: Variable replace loop. ")) { errorReport += "Error: Variable replace loop. "; }
				} else {
					if (!errorReport.Contains("Error: Missing operator. ")) { errorReport += "Error: Missing operator. "; }
				}
			}
		} else {
			equationsWithParas = SpaceKiller(equationsWithParas);
		}
		if (equationsWithParas.Contains(")") == true || equationsWithParas.Contains("(") == true && errorReport == "") {		//done to avoid unnecessary parantheses searches
			ParathesesKiller();
		} else {
			paraString = equationsWithParas;
		}
		if (errorReport == "") {
			ParsingFunction();
		}
// Final Answer
		if (errorReport == "") {
			if (numbers.Count == 1 && operators.Count == 0) {
                string finalStringAnswer = numbers[0].Real.ToString().Replace("E+", "E");
                if (numbers[0].Imaginary != 0.0) { finalStringAnswer += "+" + numbers[0].Imaginary.ToString().Replace("E+", "E") + "<i>i</i>"; }
				PlayerPrefs.SetString("PreviousResult",finalStringAnswer);
                return finalStringAnswer;
			} else if (numbers.Count == 0) {
				return "Error: FinAns, No Numbers";
			} else {
				return "Error: fin Number/Operator Count Mismatch";
			}
		} else {
            if (errorReport.Substring(errorReport.Length-1, 1) == " ") { errorReport = errorReport.Substring(0, errorReport.Length-1); }
			return errorReport;
		}
	}

    public string MultiplicationFlex (string beforeFlex) {
        string afterFlex = "";
        afterFlex = beforeFlex.Replace("1<i>", "1*<i>").Replace("2<i>", "2*<i>").Replace("3<i>", "3*<i>").Replace("4<i>", "4*<i>").Replace("5<i>", "5*<i>").Replace("6<i>", "6*<i>").Replace("7<i>", "7*<i>").Replace("8<i>", "8*<i>").Replace("9<i>", "9*<i>").Replace("0<i>", "0*<i>");
        afterFlex = afterFlex.Replace("</i>1", "</i>*1").Replace("</i>2", "</i>*2").Replace("</i>3", "</i>*3").Replace("</i>4", "</i>*4").Replace("</i>5", "</i>*5").Replace("</i>6", "</i>*6").Replace("</i>7", "</i>*7").Replace("</i>8", "</i>*8").Replace("</i>9", "</i>*9").Replace("</i>0", "</i>*0");
        afterFlex = afterFlex.Replace("-<i>i</i>", "(-1*<i>i</i>)");
        afterFlex = afterFlex.Replace("</i><i>", "</i>*<i>");
        afterFlex = afterFlex.Replace("1(", "1*(").Replace("2(", "2*(").Replace("3(", "3*(").Replace("4(", "4*(").Replace("5(", "5*(").Replace("6(", "6*(").Replace("7(", "7*(").Replace("8(", "8*(").Replace("9(", "9*(").Replace("0(", "0*(");
        afterFlex = afterFlex.Replace(")1", ")*1").Replace(")2", ")*2").Replace(")3", ")*3").Replace(")4", ")*4").Replace(")5", ")*5").Replace(")6", ")*6").Replace(")7", ")*7").Replace(")8", ")*8").Replace(")9", ")*9").Replace(")0", ")*0");
        return afterFlex;
    }

	public bool VariableReplacerValidater(string equation) {
		equation = SpaceKiller(equation.Replace("‑","-"));  // replaces non break hypen with normal one. Also kills spaces
		string unacceptables = "1234567890.#@";										//these characters should never be directly next to a variable
		varReplaceLoopSaver = 0;                                                    //loop ender in case weird stuff happens 
        string checkVariable = equation.Replace("<i>i</i>","$");                    //i next to a number/variable is acceptable
        checkVariable = checkVariable.Replace("<i>","#").Replace("</i>","@");		//replaces with a single character for easier coding

		while (checkVariable.Contains("#")==true && varReplaceLoopSaver<1000) {
			int endVar = checkVariable.IndexOf("@");
			int startVar = checkVariable.IndexOf("#");
			int lengthVar = checkVariable.Length;
			if (startVar == 0 && endVar != lengthVar-1) {							//if a variable starts the equation
				if (unacceptables.IndexOf(checkVariable[endVar+1]) == -1) {			//checks for unacceptable characters after the variable
					checkVariable = checkVariable.Substring(endVar+1,lengthVar-endVar-1);		//starts next check after this variable
				} else {
					return false;													//if an unacceptable character is found, returns false
				}
			} else if (startVar != 0 && endVar == lengthVar-1) {					//if a variable ends the equation
				if (unacceptables.IndexOf(checkVariable[startVar-1]) == -1) {		//checks for unacceptable characters before the variable
					checkVariable = checkVariable.Substring(endVar+1,lengthVar-endVar-1);		//starts next check after this variable
				} else {
					return false;													//if an unacceptable character is found, returns false
				}
			} else if (startVar != 0 && endVar != lengthVar-1) {					//if a variable is in the middle of an equation
				if (unacceptables.IndexOf(checkVariable[startVar-1]) == -1 && unacceptables.IndexOf(checkVariable[endVar+1]) == -1) {		//checks for unacceptable characters before and after the variable
					checkVariable = checkVariable.Substring(endVar+1,lengthVar-endVar-1);		//starts next check after this variable
				} else {
					return false;													//if an unacceptable character is found, returns false
				}
			} else {																//if the equation consists of one variable and nothing else
				checkVariable = checkVariable.Substring(endVar+1,lengthVar-endVar-1);
			}
			varReplaceLoopSaver++;													//loop counter
		}
		if (varReplaceLoopSaver < 1000) {return true;} else {return false;}			//will return a false if this function loops too many times
	}

	public string VariableReplacer (string equation) {
		equationsWithParas = equation.Replace("‑","-"); // replaces non break hyphens with normals ones in case it is needed.
        equationsWithParas = MultiplicationFlex(equationsWithParas);
//        equationsWithParas = equationsWithParas.Replace("1<i>", "1*<i>").Replace("2<i>", "2*<i>").Replace("3<i>", "3*<i>").Replace("4<i>", "4*<i>").Replace("5<i>", "5*<i>").Replace("6<i>", "6*<i>").Replace("7<i>", "7*<i>").Replace("8<i>", "8*<i>").Replace("9<i>", "9*<i>").Replace("0<i>", "0*<i>");
//        equationsWithParas = equationsWithParas.Replace("</i>1", "</i>*1").Replace("</i>2", "</i>*2").Replace("</i>3", "</i>*3").Replace("</i>4", "</i>*4").Replace("</i>5", "</i>*5").Replace("</i>6", "</i>*6").Replace("</i>7", "</i>*7").Replace("</i>8", "</i>*8").Replace("</i>9", "</i>*9").Replace("</i>0", "</i>*0");
//        equationsWithParas = equationsWithParas.Replace("</i><i>", "</i>*<i>");
        equationsWithParas = SpaceKiller(equationsWithParas);
		//if (metricOn==true) {		//done for future metric/SI unit button - avoiding for now. Doesn't really seem necessary
			equationsWithParas = equationsWithParas.Replace("<i>h</i>","(6.62607015E-34)");
            equationsWithParas = equationsWithParas.Replace("<i>h/2π</i>","(1.054571817646E-34)");
			equationsWithParas = equationsWithParas.Replace("<i>g</i>","(9.80665)");
			equationsWithParas = equationsWithParas.Replace("<i>G</i>","(6.67408E-11)");
			equationsWithParas = equationsWithParas.Replace("<i>c</i>","(299792458)");
            equationsWithParas = equationsWithParas.Replace("<i>e¯</i>", "(1.602176634E-19)");
            equationsWithParas = equationsWithParas.Replace("<i>kᵦ</i>", "(1.380649E-23)");
            equationsWithParas = equationsWithParas.Replace("<i>Nₐ</i>", "(6.02214076E23)");
            equationsWithParas = equationsWithParas.Replace("<i>mₚ</i>", "(1.672621898E-27)");
            equationsWithParas = equationsWithParas.Replace("<i>mₙ</i>", "(1.674927471E-27)");
            equationsWithParas = equationsWithParas.Replace("<i>mₑ</i>", "(9.1093837015E-31)");
            equationsWithParas = equationsWithParas.Replace("<i>R</i>", "(8.31446261815324)");
            equationsWithParas = equationsWithParas.Replace("<i>M⊕</i>", "(5.9722E24)");
            equationsWithParas = equationsWithParas.Replace("<i>M⊙</i>", "(1.9885E30)");
            equationsWithParas = equationsWithParas.Replace("<i>Kₑ</i>", "(8.9875517923E9)");
        //} else {}
        equationsWithParas = equationsWithParas.Replace("<i>e</i>", "(2.71828182845904523536)");		//manually putting in numbers is quicker than calling them
        equationsWithParas = equationsWithParas.Replace("<i>π</i>", "(3.14159265358979323846)");      //manually putting in numbers is quicker than calling them

    //vvv could make better? One variable shouldn't need to call 5. 
		if (equationsWithParas.Contains("Var") || equationsWithParas.Contains("Ans")) {					//PlayerPrefs hits are more time consuming than above. if no playerprefs type variables, then avoids
			equationsWithParas = equationsWithParas.Replace("<i>Ans</i>","("+PlayerPrefs.GetString("PreviousResult","0")+")");
			equationsWithParas = equationsWithParas.Replace("<i>VarA</i>","("+PlayerPrefs.GetString("VariableA","0")+")");
			equationsWithParas = equationsWithParas.Replace("<i>VarB</i>","("+PlayerPrefs.GetString("VariableB","0")+")");
			equationsWithParas = equationsWithParas.Replace("<i>VarC</i>","("+PlayerPrefs.GetString("VariableC","0")+")");
			equationsWithParas = equationsWithParas.Replace("<i>VarD</i>","("+PlayerPrefs.GetString("VariableD","0")+")");
		}
        // done again in case VarA/B/C/D/Ans is a complex number
        MultiplicationFlex(equationsWithParas);
//        equationsWithParas = equationsWithParas.Replace("1<i>", "1*<i>").Replace("2<i>", "2*<i>").Replace("3<i>", "3*<i>").Replace("4<i>", "4*<i>").Replace("5<i>", "5*<i>").Replace("6<i>", "6*<i>").Replace("7<i>", "7*<i>").Replace("8<i>", "8*<i>").Replace("9<i>", "9*<i>").Replace("0<i>", "0*<i>");
//        equationsWithParas = equationsWithParas.Replace("</i>1", "</i>*1").Replace("</i>2", "</i>*2").Replace("</i>3", "</i>*3").Replace("</i>4", "</i>*4").Replace("</i>5", "</i>*5").Replace("</i>6", "</i>*6").Replace("</i>7", "</i>*7").Replace("</i>8", "</i>*8").Replace("</i>9", "</i>*9").Replace("</i>0", "</i>*0");
//        equationsWithParas = equationsWithParas.Replace("</i><i>", "</i>*<i>");

        // needs to be after the VarA/B/C/D/Ans replacement in case they are a complex number. Any other variable should be be replaced by an actual number before saving to the VarA/B/C/D/Ans
        equationsWithParas = equationsWithParas.Replace("<i>i</i>","$");				// $ shouldnt be used elsewhere. This gets rid of the division sign in </i> that was screwing up ParsingFunction()

		return equationsWithParas;
	}

	public string SpaceKiller(string stringWithSpaces) {
		string stringWithoutSpaces = stringWithSpaces.Replace(" ","");
		stringWithoutSpaces = stringWithoutSpaces.Replace("--","");
		return stringWithoutSpaces;
	}
	
	void ParathesesKiller() {
//		numberOfEndPara = equationsWithParas.Split(new string[] {")"},System.StringSplitOptions.None).Length-1;			//splits the string into groups divided by ). minus one off the total to get number of )
//		numberOfBeginPara = equationsWithParas.Split(new string[] {"("},System.StringSplitOptions.None).Length-1;		//splits the string into groups divided by (. minus one off the total to get number of (
//	^^^ above takes more time, apparently, then below vvv
		string[] badIfRightOfParas = new string[] {"0","1","2","3","4","5","6","7","8","9",".","E","-","$"};
		string[] badIfLeftOfParas = new string[] {"0","1","2","3","4","5","6","7","8","9",".","E","²","³","$"};

		numberOfEndPara = 0;
		numberOfBeginPara = 0;
		for (int k=0; k<equationsWithParas.Length;k++) {
			if (equationsWithParas[k] == ')') {numberOfEndPara++;}
			if (equationsWithParas[k] == '(') {numberOfBeginPara++;}
		}

		if (numberOfEndPara == numberOfBeginPara) {
			if (numberOfEndPara != 0) {
				if (equationsWithParas.Contains(")(") == true) {
					if (!errorReport.Contains("Error: Missing operator between paratheses. ")) { errorReport += "Error: Missing operator between paratheses. "; }
				}
				if (equationsWithParas.Contains("()") == true) {
					if (!errorReport.Contains("Error: Nothing in paratheses. ")) { errorReport += "Error: Nothing in paratheses. "; }
				}
				for (int i=0; i<badIfLeftOfParas.Length; i++) {
					if (equationsWithParas.Contains(badIfLeftOfParas[i]+"(") == true) {
						if (!errorReport.Contains("Error: Operator missing before paratheses. ")) { errorReport += "Error: Operator missing before paratheses. "; }
					}
				}
				for (int i=0; i<badIfRightOfParas.Length; i++) {
					if (equationsWithParas.Contains(")"+badIfRightOfParas[i]) == true) {
						if (!errorReport.Contains("Error: Operator missing after paratheses. ")) { errorReport += "Error: Operator missing after paratheses. "; }
					}
				}
//				int savior=0;
				while (equationsWithParas.Contains(")") == true && errorReport == "") {
//					savior++;
//					Debug.Log(savior);
					firstEndPara = equationsWithParas.IndexOf(")");
					prevBeginPara = -1;
					for (int k=firstEndPara; k > prevBeginPara;k--) {
						if (equationsWithParas.Substring(k,1) == "(") {
							prevBeginPara = k;
						}
					}
				//vvv fixes negative sign in front of "(" to be treated as -1*(), therefore -(5)^2 = -25 and not 25. Does not change if there is a ^ before the - because 10^-(2) was -> 10^-1.0*2=0.2
					if (prevBeginPara == 1) {
						if (equationsWithParas[0] == '-') {
							equationsWithParas = equationsWithParas.Insert(1,"1.0*");
							prevBeginPara += 4;
							firstEndPara += 4;
						}
					}
					if (prevBeginPara >= 2) {
						if (equationsWithParas[prevBeginPara-1] == '-' && equationsWithParas[prevBeginPara-2] != '^') {
							equationsWithParas = equationsWithParas.Insert(prevBeginPara,"1.0*");
							prevBeginPara += 4;
							firstEndPara += 4;
						}
					}
				//^^^
					paraString = equationsWithParas.Substring(prevBeginPara+1,firstEndPara-prevBeginPara-1);
//					string endOfParaString=equationsWithParas.Substring(firstEndPara+1,equationsWithParas.Length-firstEndPara-1);		//done to limit the number of substring calls
//					string beginOfParaString=equationsWithParas.Substring(0,prevBeginPara);												//done to limit the number of substring calls
					if (paraString.Length > 0) {
						ParsingFunction(); // parses things inside the paraenthese
//		//vvv fixes negative sign in front of "(" to be treated as (-1*()), therefore -(5)^2 = -25 and not 25. changed to (-1.0*())	from -1.0*() because 10^-(2) was -> 10^-1.0*2=0.2
//						if (prevBeginPara == 1 && equationsWithParas.Substring(prevBeginPara-1,1) == "-") {
//							equationsWithParas = equationsWithParas.Substring(0,prevBeginPara-1) + "-1.0*" + numbers[0].ToString() +endOfParaString;
//						} else if (prevBeginPara > 1 && equationsWithParas.Substring(prevBeginPara-1,1) == "-") {
//							string paraOperator=equationsWithParas.Substring(prevBeginPara-2,1);
//							if (paraOperator == "+" || paraOperator == "–" || paraOperator == "*" || paraOperator == "/" || paraOperator == "^" || paraOperator == "(") {
//								equationsWithParas = equationsWithParas.Substring(0,prevBeginPara-1) + "(-1.0*" + numbers[0].ToString() + ")" + endOfParaString;
//							} else {
//								equationsWithParas = beginOfParaString + numbers[0].ToString() + endOfParaString;
//							}
//						} else {
//							equationsWithParas = beginOfParaString + numbers[0].ToString() + endOfParaString;
//						}
						if (errorReport=="") {
                            if (numbers[0].Imaginary == 0.0) {
							    equationsWithParas = equationsWithParas.Replace("("+paraString+")",numbers[0].Real.ToString());		//replaces parentheses-equation with solved answer
                            } else {
							    equationsWithParas = equationsWithParas.Replace("("+paraString+")",(numbers[0].Real + "&" + numbers[0].Imaginary).ToString());		//replaces parentheses-equation with solved answer
                            }
							equationsWithParas = equationsWithParas.Replace("--","").Replace("E+", "E");			//gets rid of potential double negatives
						}
//		//^^^
//	//					equationsWithParas = beginOfParaString + numbers[0].ToString() + endOfParaString;
					} else {
						if (!errorReport.Contains("Error: Nothing in paratheses (2nd). ")) { errorReport += "Error: Nothing in paratheses (2nd). "; }	//shows for something like )+8+(
	//					equationsWithParas = equationsWithParas.Replace("()","");
					}
				}
			}
			paraString = equationsWithParas.Replace("E+", "E"); // could have E+'s again
			if (errorReport == "") {
//				Debug.Log(paraString);
				ParsingFunction(); // parses end result
			}
		} else {
			if (!errorReport.Contains("Error: Paratheses. ")) { errorReport += "Error: Paratheses. "; }
		}
	}

	void ParsingFunction() {                            // puts string equation into a list of numbers and a list of operators
        toBeParsed = paraString;
    Debug.Log(toBeParsed);
//		numbers = new List<double> {};
		numbers = new List<Complex> {};
		operators = new List<string> {};

		if (toBeParsed.Length == 0) {
			if (!errorReport.Contains("Error: No equation. ")) { errorReport += "Error: No equation. "; }
		}
		
		for (int j=0; j<toBeParsed.Length;j++) {
			bool operationMatch = false;
			bool negativeNumAdd = false;
			bool addBufferNumber = false;
			int currentEqnLength = toBeParsed.Length;
			string tempOperator = toBeParsed[j].ToString();
			if (numberParts.IndexOf(tempOperator) == -1) {													//if it finds a piece of string that is not a piece of a number, therefore is an operator
	//			Debug.Log(toBeParsed+", "+j);
				if (j != 0) {				//done in case there is no number before the operator
					if (j == 1 && toBeParsed[0] == '-') { // add negative number because a negative sign was placed in front of an operator
						negativeNumAdd = true;
					}
                    //      else if (double.TryParse(toBeParsed.Substring(0,j),out standByNum) == true) {
                    //			numbers.Add(standByNum);
                    //		} 
                    if (toBeParsed.Substring(0,j).Contains("&")) {
                        string tempString = toBeParsed.Substring(0, j);
                        string[] splitString = tempString.Split('&');
                        double firstPart;
                        double secondPart;
                        if (double.TryParse(splitString[0],out firstPart) == true && double.TryParse(splitString[1],out secondPart) == true) {
                            numbers.Add(new Complex(firstPart,secondPart));
				        } else { if (!errorReport.Contains("Error: NaN in eqn (3rd). ")) { errorReport += "Error: NaN in eqn (3rd). "; } }
                    } else if (double.TryParse(toBeParsed.Substring(0,j),out standByNumDub) == true) {
						numbers.Add(new Complex(standByNumDub,0.0));																				//couter for graphing to replace x
					} else {
						if (!errorReport.Contains("Error: NaN in eqn (2nd). ")) { errorReport += "Error: NaN in eqn (2nd). "; }
					}
				}
				if (tempOperator == "+" || tempOperator == "–" || tempOperator == "*" || tempOperator == "/" || tempOperator == "^") {
					operators.Add(tempOperator);
					operationMatch = true;
					j += 1;
				}
				if (operationMatch == false && currentEqnLength-j >= 6) {
					tempOperator = toBeParsed.Substring(j,6);														//done to save multiple computations of the same thing
					if (tempOperator == "Cosh¯¹" || tempOperator == "Sinh¯¹" || tempOperator == "Tanh¯¹" ||
						 tempOperator == "Sech¯¹" || tempOperator == "Csch¯¹" || tempOperator == "Coth¯¹") {
						operators.Add(tempOperator);
						j += 6;
						addBufferNumber = true;																		//added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
						operationMatch = true;
					}
				}
				if (operationMatch == false && currentEqnLength-j >= 5) {
					tempOperator = toBeParsed.Substring(j,5);														//done to save multiple computations of the same thing
					if (tempOperator == "Cos¯¹" || tempOperator == "Sin¯¹" || tempOperator == "Tan¯¹" ||
						 tempOperator == "Sec¯¹" || tempOperator == "Csc¯¹" || tempOperator == "Cot¯¹") {
						operators.Add(tempOperator);
						j += 5;
						addBufferNumber = true;																		//added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
						operationMatch = true;
					}
				}
				if (operationMatch == false && currentEqnLength-j >= 4) {
					tempOperator = toBeParsed.Substring(j,4);														//done to save multiple computations of the same thing
					if (tempOperator == "Cosh" || tempOperator == "Sinh" || tempOperator == "Tanh" ||
						 tempOperator == "Sech" || tempOperator == "Csch" || tempOperator == "Coth" ||
						 tempOperator == "Ceil") {
						operators.Add(tempOperator);
						j += 4;
						addBufferNumber = true;																		//added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
						operationMatch = true;
					}
				}
				if (operationMatch == false && currentEqnLength-j >= 3) {
					tempOperator = toBeParsed.Substring(j,3);														//done to save multiple computations of the same thing
					if (tempOperator == "Cos" || tempOperator == "Sin" || tempOperator == "Tan" || 
						 tempOperator == "Sec" || tempOperator == "Csc" || tempOperator == "Cot" || 
						 tempOperator == "Abs" || tempOperator == "log" || tempOperator == "Flr" || 
						 tempOperator == "Rnd" ) {
						operators.Add(tempOperator);
						j += 3;
						addBufferNumber = true;																		//added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
						operationMatch = true;
					}
				}
				if (operationMatch == false && currentEqnLength-j >= 2) {
					tempOperator = toBeParsed.Substring(j,2);														//done to save multiple computations of the same thing
					if (tempOperator == "ln" || tempOperator == "¯¹") {
						operators.Add(tempOperator);
						j += 2;
						addBufferNumber = true;																		//added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
						operationMatch = true;
					}
				}
				if (operationMatch == false && currentEqnLength-j >= 1) {
					tempOperator = toBeParsed[j].ToString();														//done to save multiple computations of the same thing
					if (tempOperator == "√" || tempOperator == "%" || tempOperator == "²" || tempOperator == "³" || 
                         tempOperator == "!" || tempOperator == "$") {
						if (j == 0) {
				//			if (tempOperator != "√" && tempOperator != "$") {           // this error message got in the way for i² and i³. Potential erros with these powers appear to be caught be the number/operational mismatch catch. Unsure if this error is needed. Seems to work fine without it in. 
				//				errorReport += "Error: Missing number. ";
				//			} else {
								addBufferNumber = true;                                                         //added in to keep numbers.count one more than operators count. These additions are just fillers and will be multiplied away
                //            }	
                            if (tempOperator == "$") {
                                numbers.Add(1.0);
                            }		
						} else {					
							addBufferNumber = true;
						}
						operators.Add(tempOperator);
						j += 1;
						operationMatch = true;
					}
				}
				if (addBufferNumber == true) {					// adds -1 or +1 buffer number
					if (negativeNumAdd == true) {
						numbers.Add(-1.0);
					} else {
						numbers.Add(1.0);
					}
				}
				if (operationMatch == true) {
					toBeParsed = toBeParsed.Substring(j,currentEqnLength-j);
					j = -1;
				} else {
                    Debug.Log(toBeParsed);
					if (!errorReport.Contains("Error: Unknown operator. ")) { errorReport += "Error: Unknown operator. "; }
				}
			}
			if (j == currentEqnLength-1) { //this is when we are at the end of the eqn and won't hit an operator to tell us a new term has started - then check the last part to see if it is a number
	            if (toBeParsed.Substring(0,j+1).Contains("&")) {
                    string tempString = toBeParsed.Substring(0, j + 1);
                    string[] splitString = tempString.Split('&');
                    double firstPart;
                    double secondPart;
                    if (double.TryParse(splitString[0],out firstPart) == true && double.TryParse(splitString[1],out secondPart) == true) {
                        numbers.Add(new Complex(firstPart,secondPart));
				    } else { if (!errorReport.Contains("Error: NaN in eqn (4th). ")) { errorReport += "Error: NaN in eqn (4th). "; } }
                } else if (double.TryParse(toBeParsed.Substring(0,j+1),out standByNumDub) == true) {
					numbers.Add(new Complex(standByNumDub,0.0));
				} else { if (!errorReport.Contains("Error: NaN at end. ")) { errorReport += "Error: NaN at end. "; } }
			}
		}

        if (numbers.Count == operators.Count+1) {
//			Debug.Log("Good Count!");
			if (outsiderUse==false) {	
				FinalSolver();
			}
		} else if (numbers.Count == 0) {
			if (!errorReport.Contains("Error: No numbers. ")) { errorReport += "Error: No numbers. "; }
		} else {
			if (!errorReport.Contains("Error: Number/Operator count mismatch. ")) { errorReport += "Error: Number/Operator count mismatch. "; }
//			Debug.Log("Bad Count! Number Count: "+numbers.Count+", Ops Count: "+operators.Count);
//			Debug.Log("Number 0: "+numbers[0]+", Number 1: "+numbers[1]);
//			Debug.Log("Op 0: "+operators[0]+", Op 1: "+operators[1]);
		}
	}

	public bool OutsideScriptEquationValidater (string outsideEquation) {	//used for in GraphYKeyboard script to quick check equations before graphing. Use when validating equations but not needing to use EquationSolver() 
		errorReport = "";
//		numbers = new List<double> {};
		numbers = new List<Complex> {};
		operators = new List<string> {};
		varReplaceLoopSaver = 0;
		outsiderUse=true;
        outsideEquation = outsideEquation.Replace("‑", "-"); // don't think this will ever be needed. Replaces non break hyphen with a normal one
		if (outsideEquation.Contains("</i>") == true) {
			bool checker=VariableReplacerValidater(outsideEquation);
			if (checker==true) {
				outsideEquation=VariableReplacer(outsideEquation);
			} else {
				outsiderUse=false;
				return false;
			}
		} else {
			outsideEquation=SpaceKiller(outsideEquation);
		}
		equationsWithParas=outsideEquation;
		ParathesesKiller();
		if (errorReport == "" && numbers.Count == operators.Count+1) {
			outsiderUse=false;
			return true;
		} else {
			outsiderUse=false;
			return false;
		}
	}

    public Complex ComplexAccumulaionErrorEliminator (Complex complexNumber) { // seems to give out around i^10000. This is probably good enough, though. - could multiply it by the power?
        if (System.Math.Abs(complexNumber.Real) > System.Math.Abs(1E12*complexNumber.Imaginary)) { // 1 000 000 000 000 - one trillion
            return new Complex(complexNumber.Real,0.0);
        } else if (System.Math.Abs(complexNumber.Real*1E12) < System.Math.Abs(complexNumber.Imaginary)) { // 1 000 000 000 000 - one trillion
            return new Complex(0.0,complexNumber.Imaginary);
        } else {
            return complexNumber;
        }
    }

	void FinalSolver() {
// once numbers and operators are correctly in arrays

	//		Debug.Log("Count! Number Count: "+numbers.Count+", Ops Count: "+operators.Count);
    //    int loopsaver5000 = 0;
		while (operators.Count > 0 && errorReport == "") { // the part of errorReport == "" is a bonus protection against a forever loop. should be handled in function itself by removing the operator everytime even if error occurs
	//		loopsaver5000++;
	//		if (loopsaver5000 >= 20) {
	//			Debug.Log("Loop Saver 5000!!!");
    //           errorReport += "Error: FinSol Loopsaver5000";
	//		}

// is comment below still relevant? it appears all operators are being deleted eventually
// Operators that include parantheses (ex. Cos(), Abs(), log(),) numbers[i] and numbers[i+1] can NOT be deleted
// because they may be a -1 due to how negatives and minus signs have to work

			for (int i=0; i < operators.Count; i++) {
				 if (operators[i] == "$") { // used to convert imaginary parts of complex numbers into a complex number
                    standByNum = numbers[i+1]*new Complex(0.0,numbers[i].Real);
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "¯¹") {
                    if (numbers[i].Imaginary == 0.0) {
					    standByNum = System.Math.Pow(numbers[i].Real,-1.0)*numbers[i+1];
                    } else {
					    standByNum = Complex.Pow(numbers[i],-1.0)*numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "²") {
                    if (numbers[i].Imaginary == 0.0) {
			    		standByNum = System.Math.Pow(numbers[i].Real,2.0)*numbers[i+1];
                    } else {
					    standByNum = Complex.Pow(numbers[i],2.0)*numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "³") {
                    if (numbers[i].Imaginary == 0.0) {
			    		standByNum = System.Math.Pow(numbers[i].Real,3.0)*numbers[i+1];
                    } else {
					    standByNum = Complex.Pow(numbers[i],3.0)*numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "%") {
                    if (numbers[i].Imaginary == 0.0 && numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i].Real*0.01*numbers[i+1].Real;
                    } else {
					    standByNum = numbers[i]*0.01*numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "√") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i+1].Real >= 0.0) {
					    standByNum = numbers[i]*System.Math.Pow(numbers[i+1].Real,0.5);
                    } else if (numbers[i+1].Imaginary == 0.0 && numbers[i+1].Real < 0.0) {
                        standByNum = new Complex(0.0,System.Math.Pow(numbers[i+1].Real*-1.0,0.5));
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    } else {
					    standByNum = numbers[i]*Complex.Pow(numbers[i+1],0.5);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "!") {
                    if (numbers[i].Imaginary == 0.0) {
					    if (numbers[i] == Mathf.FloorToInt((float)numbers[i].Real) && numbers[i].Real >= 0) {
						    standByNum = 1.0*numbers[i+1];
						    for (int fact=(int)numbers[i].Real; fact>0; fact--) {
							    standByNum *= fact;
						    }
						    standByNum *= numbers[i+1];
                            // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                            if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20 &&
                                System.Math.Abs(numbers[i].Real) < 1E20 && System.Math.Abs(numbers[i].Real) > 1E-20 &&
                                System.Math.Abs(numbers[i+1].Real) < 1E20 && System.Math.Abs(numbers[i+1].Real) > 1E-20) {
                                    decimal tempStandBy = 1m*(decimal)numbers[i+1].Real;
						            for (int fact = (int)numbers[i].Real; fact>0; fact--) {
							            tempStandBy *= fact;
						            }
						            tempStandBy *= (decimal)numbers[i+1].Real;
                                    standByNum = new Complex((double)tempStandBy,0.0);
                            }
						    numbers.RemoveAt(i);
						    numbers[i] = standByNum;
						    operators.RemoveAt(i);
						    i--;
					    } else {
						    if (!errorReport.Contains("Error: Factorial. ")) { errorReport += "Error: Factorial. "; }
						    operators.RemoveAt(i);
					    }
                    } else {
                        if (!errorReport.Contains("Error: Can not use Factorial on a complex numbers. ")) { errorReport += "Error: Can not use Factorial on a complex numbers. "; }
						operators.RemoveAt(i);
                    }
                    /*
					if (numbers[i] == Mathf.FloorToInt((float)numbers[i]) && numbers[i] >= 0) {
						standByNum = 1.0*numbers[i+1];
						for (int fact = (int)numbers[i]; fact>0; fact--) {
							standByNum *= fact;
						}
						standByNum *= numbers[i+1];
						numbers.RemoveAt(i);
						numbers[i] = standByNum;
						operators.RemoveAt(i);
						i--;
					} else {
						errorReport += "Error: Factorial. ";
						operators.RemoveAt(i);
					}
                    */
				} else if (operators[i] == "Cos") {
                    //could put in standard coversions for trig in this area for each function - ex. cos(pi/2) = 0, sin(pi) = 0 (instead of sin(pi) = 3.2E-15)
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Cos(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Cos(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Cos(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Cos(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sin") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Sin(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Sin(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Sin(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Sin(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Tan") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Tan(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Tan(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Tan(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Tan(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sec") {
                    if (numbers[i+1].Imaginary == 0.0) {
				    	if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/System.Math.Cos(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]/System.Math.Cos(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/Complex.Cos(numbers[i+1]);}
					     else {standByNum = numbers[i]/Complex.Cos(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Csc") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/System.Math.Sin(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]/System.Math.Sin(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/Complex.Sin(numbers[i+1]);}
					     else {standByNum = numbers[i]/Complex.Sin(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Cot") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/System.Math.Tan(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]/System.Math.Tan(numbers[i+1].Real*3.1415926535897932/180.0);}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]/Complex.Tan(numbers[i+1]);}
					     else {standByNum = numbers[i]/Complex.Tan(numbers[i+1]*3.1415926535897932/180.0);}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Cosh") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Cosh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Cosh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sinh") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Sinh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Sinh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Tanh") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Tanh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Tanh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sech") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]/System.Math.Cosh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]/Complex.Cosh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Csch") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]/System.Math.Sinh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]/Complex.Sinh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Coth") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]/System.Math.Tanh(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]/Complex.Tanh(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Cos¯¹") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Acos(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Acos(numbers[i+1].Real)*180.0/3.1415926535897932;}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Acos(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Acos(numbers[i+1])*180.0/3.1415926535897932;}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sin¯¹") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Asin(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Asin(numbers[i+1].Real)*180.0/3.1415926535897932;}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Asin(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Asin(numbers[i+1])*180.0/3.1415926535897932;}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Tan¯¹") {
                    if (numbers[i+1].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*System.Math.Atan(numbers[i+1].Real);}
			    		 else {standByNum = numbers[i]*System.Math.Atan(numbers[i+1].Real)*180.0/3.1415926535897932;}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = numbers[i]*Complex.Atan(numbers[i+1]);}
					     else {standByNum = numbers[i]*Complex.Atan(numbers[i+1])*180.0/3.1415926535897932;}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sec¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = System.Math.Acos(numbers[i].Real/numbers[i+1].Real);}
			    		 else {standByNum = System.Math.Acos(numbers[i].Real/numbers[i+1].Real)*180.0/3.1415926535897932;}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = Complex.Acos(numbers[i]/numbers[i+1]);}
					     else {standByNum = Complex.Acos(numbers[i]/numbers[i+1])*180.0/3.1415926535897932;}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Csc¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = System.Math.Asin(numbers[i].Real/numbers[i+1].Real);}
			    		 else {standByNum = System.Math.Asin(numbers[i].Real/numbers[i+1].Real)*180.0/3.1415926535897932;}
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {standByNum = Complex.Asin(numbers[i]/numbers[i+1]);}
					     else {standByNum = Complex.Asin(numbers[i]/numbers[i+1])*180.0/3.1415926535897932;}
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Cot¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
			    		if (PlayerPrefsX.GetBool("InRadians",true) == true) {if (numbers[i+1].Real>=0) {standByNum = System.Math.Atan(numbers[i].Real/numbers[i+1].Real);} else {standByNum = System.Math.Atan(numbers[i].Real/numbers[i+1].Real)+3.1415926535897932;} }
			    		 else {if (numbers[i+1].Real>=0) {standByNum = System.Math.Atan(numbers[i].Real/numbers[i+1].Real)*180.0/3.1415926535897932;} else {standByNum = System.Math.Atan(numbers[i].Real/numbers[i+1].Real)*180.0/3.1415926535897932+180.0;} }
                    } else {
					    if (PlayerPrefsX.GetBool("InRadians",true) == true) {if (numbers[i+1].Real>=0) {standByNum = Complex.Atan(numbers[i]/numbers[i+1]);} else {standByNum = Complex.Atan(numbers[i]/numbers[i+1])+3.1415926535897932;} }
					     else {if (numbers[i+1].Real>=0) {standByNum = Complex.Atan(numbers[i]/numbers[i+1])*180.0/3.1415926535897932;} else {standByNum = Complex.Atan(numbers[i]/numbers[i+1])*180.0/3.1415926535897932+180.0;} }
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Cosh¯¹") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Log(numbers[i+1].Real+System.Math.Sqrt(numbers[i+1].Real*numbers[i+1].Real-1));
                    } else {
					    standByNum = numbers[i]*Complex.Log(numbers[i+1]+Complex.Sqrt(numbers[i+1]*numbers[i+1]-1));
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sinh¯¹") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Log(numbers[i+1].Real+System.Math.Sqrt(numbers[i+1].Real*numbers[i+1].Real+1));
                    } else {
					    standByNum = numbers[i]*Complex.Log(numbers[i+1]+Complex.Sqrt(numbers[i+1]*numbers[i+1]+1));
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Tanh¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = (numbers[i].Real/2.0)*System.Math.Log((1+numbers[i+1].Real)/(1-numbers[i+1].Real));
                    } else {
					    standByNum = (numbers[i]/2.0)*Complex.Log((1+numbers[i+1])/(1-numbers[i+1]));
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Sech¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real*System.Math.Log( (1.0/numbers[i+1].Real)+(System.Math.Sqrt((1.0/numbers[i+1].Real)*(1.0/numbers[i+1].Real)-1)) );
                    } else {
					    standByNum = numbers[i]*Complex.Log( (1.0/numbers[i+1])+(Complex.Sqrt((1.0/numbers[i+1])*(1.0/numbers[i+1])-1)) );
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Csch¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real*System.Math.Log( (1.0/numbers[i+1].Real)+(System.Math.Sqrt((1.0/numbers[i+1].Real)*(1.0/numbers[i+1].Real)+1)) );
                    } else {
					    standByNum = numbers[i]*Complex.Log( (1.0/numbers[i+1])+(Complex.Sqrt((1.0/numbers[i+1])*(1.0/numbers[i+1])+1)) );
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Coth¯¹") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = (numbers[i].Real/2.0)*System.Math.Log((numbers[i+1].Real+1)/(numbers[i+1].Real-1));
                    } else {
					    standByNum = (numbers[i]/2.0)*Complex.Log((numbers[i+1]+1)/(numbers[i+1]-1));
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Abs") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Abs(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Abs(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "log") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Log10(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Log10(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "ln") {
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Log(numbers[i+1].Real);
                    } else {
					    standByNum = numbers[i]*Complex.Log(numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Flr") {
			//		standByNum = numbers[i]*System.Math.Floor(numbers[i+1]);
                    if (numbers[i+1].Imaginary == 0.0) {
                        standByNum = numbers[i]*System.Math.Floor(numbers[i+1].Real);
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20) {
                            decimal tempStandBy = (decimal)numbers[i].Real*decimal.Floor((decimal)numbers[i+1].Real);
                            standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
                        if (!errorReport.Contains("Error: Can not Floor complex numbers. ")) { errorReport += "Error: Can not Floor complex numbers. "; }
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Ceil") {
			//		standByNum = numbers[i]*System.Math.Ceiling(numbers[i+1]);
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Ceiling(numbers[i+1].Real);
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20) {
                            decimal tempStandBy = (decimal)numbers[i].Real*decimal.Ceiling((decimal)numbers[i+1].Real);
                            standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
                        if (!errorReport.Contains("Error: Can not Ceiling complex numbers. ")) { errorReport += "Error: Can not Ceiling complex numbers. "; }
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "Rnd") {
			//		standByNum = numbers[i]*System.Math.Round(numbers[i+1]);
                    if (numbers[i+1].Imaginary == 0.0) {
					    standByNum = numbers[i]*System.Math.Round(numbers[i+1].Real);
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20) {
                            decimal tempStandBy = (decimal)numbers[i].Real*decimal.Round((decimal)numbers[i+1].Real);
                            standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
                        if (!errorReport.Contains("Error: Can not Round complex numbers. ")) { errorReport += "Error: Can not Round complex numbers. "; }
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				}
			}
// for exponents
			for (int i=0; i < operators.Count; i++) {
				if (operators[i] == "^") {
   //                 if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
//		    		standByNum = System.Math.Pow((double)numbers[i].Real,(double)numbers[i+1].Real);
//                    } else {
					    standByNum = Complex.Pow(numbers[i],numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
//                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				}
			}
// for multiplication and division
			for (int i=0; i < operators.Count; i++) {
				if (operators[i] == "*") { // won't need x or X because user will be using calc keyboard which will put multiplication as *
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real*numbers[i+1].Real;
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20 &&
                            System.Math.Abs(numbers[i].Real) < 1E20 && System.Math.Abs(numbers[i].Real) > 1E-20 &&
                            System.Math.Abs(numbers[i+1].Real) < 1E20 && System.Math.Abs(numbers[i+1].Real) > 1E-20) {
                                decimal tempStandBy = (decimal)numbers[i].Real*(decimal)numbers[i+1].Real;
                                standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
					    standByNum = numbers[i]*numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				}
 				else if (operators[i] == "/") {//doing division by muliplying the inverted next number fixes problems
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real*(1.0/numbers[i+1].Real);
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20 &&
                            System.Math.Abs(numbers[i].Real) < 1E20 && System.Math.Abs(numbers[i].Real) > 1E-20 &&
                            System.Math.Abs(numbers[i+1].Real) < 1E20 && System.Math.Abs(numbers[i+1].Real) > 1E-20) {
                                decimal tempStandBy = (decimal)numbers[i].Real*(1m/(decimal)numbers[i+1].Real);
                                standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
					    standByNum = numbers[i]*(1.0/numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				}
			}
// for addition and subtraction
			for (int i=0; i < operators.Count; i++) { 
				if (operators[i] == "–") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real+(-1.0*numbers[i+1].Real);
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20 &&
                            System.Math.Abs(numbers[i].Real) < 1E20 && System.Math.Abs(numbers[i].Real) > 1E-20 &&
                            System.Math.Abs(numbers[i+1].Real) < 1E20 && System.Math.Abs(numbers[i+1].Real) > 1E-20) {
                                decimal tempStandBy = (decimal)numbers[i].Real+(-1m*(decimal)numbers[i+1].Real);
                                standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
					    standByNum = numbers[i]+(-1.0*numbers[i+1]);
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				} else if (operators[i] == "+") {
                    if (numbers[i+1].Imaginary == 0.0 && numbers[i].Imaginary == 0.0) {
					    standByNum = numbers[i].Real+numbers[i+1].Real;
                        // below is done to keep numbers as accurate as possible was seeing things like 61.752-61=0.75200000000002
                        if (System.Math.Abs(standByNum.Real) < 1E20 && System.Math.Abs(standByNum.Real) > 1E-20 &&
                            System.Math.Abs(numbers[i].Real) < 1E20 && System.Math.Abs(numbers[i].Real) > 1E-20 &&
                            System.Math.Abs(numbers[i+1].Real) < 1E20 && System.Math.Abs(numbers[i+1].Real) > 1E-20) {
                                decimal tempStandBy = (decimal)numbers[i].Real+(decimal)numbers[i+1].Real;
                                standByNum = new Complex((double)tempStandBy,0.0);
                        }
                    } else {
					    standByNum = numbers[i]+numbers[i+1];
                        standByNum = ComplexAccumulaionErrorEliminator(standByNum);
                    }
					numbers.RemoveAt(i);
					numbers[i] = standByNum;
					operators.RemoveAt(i);
					i--;
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
