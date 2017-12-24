using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Diagnostics;

namespace ForumMafiaRestOnly
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IVoteCountRest
    {


        private const string URL_OF_GAME_PROMPT = "url=";
        private const string PLAYER_TEXT_PROMPT = "playerList=";
        private const string REPLACEMENTS_LIST_PROMPT = "replacementList=";
        private const string MOD_LIST_PROMPT = "moderatorNames=";
        private const string DAY_NUMBERS_PROMPT = "dayStartNumbers=";
        private const string DEAD_LIST_PROMPT = "deadList=";
        private const string DEADLINE_PROMPT = "deadline=";
        private const string FLAVOR_PROMPT = "flavor=";
        private const string VOTE_OVERRIDES_PROMPT = "voteOverrides=";
        private const string ALPHA_SORT_PROMPT = "alphaSort=";
        private const string SIMPLE_SORT_PROMPT = "simple=";
        private const string L_SORT_PROMPT = "lSort=";
        private const string CLEAN_DAY_PROMPT = "cleanDay=";
        private const string DISPLAY_ALL_VCs_PROMPT = "displayAllVCs=";
        private const string VOTE_NUMBER_INPUT = "priorVCNumber=";

        private static string[] ALL_PROMPTS = new string[] { VOTE_NUMBER_INPUT, URL_OF_GAME_PROMPT, PLAYER_TEXT_PROMPT, DEADLINE_PROMPT, REPLACEMENTS_LIST_PROMPT, MOD_LIST_PROMPT, DAY_NUMBERS_PROMPT, DEAD_LIST_PROMPT, FLAVOR_PROMPT, VOTE_OVERRIDES_PROMPT, ALPHA_SORT_PROMPT, SIMPLE_SORT_PROMPT, L_SORT_PROMPT, CLEAN_DAY_PROMPT, DISPLAY_ALL_VCs_PROMPT };


        //Constants formatting
        public static char COMMA = ',';
        public static char COLON = ':';

        //Input variables
        private string playerText;
        private string replacementText;
        private string dayPostStartList;
        private string deadPlayerListText;
        private string moderatorNamesText;
        private string priorVCNumberInputText;
        private string voteOverrideText;


        //Variables used in data crunching.
        string urlOfGame;
        List<Player> players;
        List<Replacement> replacements;
        List<string> moderatorNames;
        List<int> dayStartPostNumbers;
        List<List<Player>> nightkilledPlayers;
        int priorVCNumber;
        private string flavorText;
        private string deadlineCode;
        List<Vote> votesByOverride;

        public string Ping()
        {
            return "Ping received";
        }

        public string UpCheck(string message)
        {
            return "You sent the test message " + message;
        }

        

        public string GetVotes(bool isRestCall, string url, string playerList, string replacementList, string moderatorNamesInput, string dayStartList = null, string deadListText = null, string priorVCNumberInput = null, string optionalFlavorText = null, string optionalDeadlineText = null, string optionalVoteOverrideData = null, string playerName = null)
        {
            
            VoteServiceObject vso = BuildBaseVoteObject(isRestCall, url, playerList, replacementList, moderatorNamesInput, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
            if (vso.ErrorMessage != null)
            {
                return vso.ErrorMessage;
            }
            else
            {
                if (!string.IsNullOrEmpty(playerName))
                {
                    Player player = Player.FindPlayerByNameUserAidReplacementsLoop(vso.Players, playerName, vso.Replacements);
                    if (player != null)
                    {
                        return vso.VoteStringByPlayer(player);
                    }
                    else
                    {
                        return "Player named " + playerName + " not found. Please check data entry.";
                    }
                }
                else
                {
                    return vso.AllVotesString;
                }
            }

        }

        public string GetCurrentVoteCountModPostAPI(string url, string postNumber)
        {
            try
            {
                return GetCurrentVoteCountModPost(url, int.Parse(postNumber));
            }
            catch
            {
                return "An error occurred attempting to process your request. Please check your data and try again.";
            }
        }

        public string GetVotesByPlayerJSON(string url, string postNumber, string playerName)
        {
            try
            {
                return GetVoteJSONModPost(url, int.Parse(postNumber), playerName);
            }
            catch
            {
                return "An error occurred attempting to process your request. Please check your data and try again.";
            }
        }

        public string GetVotesJSON(string url, string postNumber)
        {
            try
            {
                return GetVoteJSONModPost(url, int.Parse(postNumber));
            }
            catch
            {
                return "An error occurred attempting to process your request. Please check your data and try again.";
            }
        }

        public string GetVotesJSONByPlayer(string url, string postNumber, string playerName)
        {
            if (!string.IsNullOrEmpty(playerName))
            {
                return GetVoteJSONModPost(url, int.Parse(postNumber), playerName);

            }
            else
            {
                return GetVotesJSON(url, postNumber);
            }
        }

        public string GetHistories(string url, string postNumber, string allHistories)
        {
            try
            {
                return GetHistoriesFromModPost(url, int.Parse(postNumber), Boolean.Parse(allHistories));
            }
            catch
            {
                return "An error occurred attempting to process your request. Please check your data and try again.";
            }
        }

        public string GetHistoriesFromModPost(string url, int postNumber, bool allHistories)
        {
            string settingsString = RunScrubLogic.ScrubPostForSettings(url, postNumber);
            return GetHistoriesFromSettingsString(true, settingsString, allHistories);
        }

        public string GetVoteJSONModPost(string url, int postNumber, string playerName = null)
        {
            string settingsString = RunScrubLogic.ScrubPostForSettings(url, postNumber);
            return GetVoteJSONFromSettingsString(true, settingsString, playerName);
        }

        public string GetCurrentVoteCountModPost(string url, int postNumber)
        {
            
            string settingsString = RunScrubLogic.ScrubPostForSettings(url, postNumber);
            return GetModCountFromSettingsString(true, settingsString);
            
            
        }

        public string GetVotesDebug(string url, string playerList, string replacementList, string moderatorNamesInput, string dayStartList = null, string deadListText = null, string priorVCNumberInput = null, string optionalFlavorText = null, string optionalDeadlineText = null, string optionalVoteOverrideData = null)
        {

            return RunScrubLogic.GetVotesDebug(url, playerList, replacementList, moderatorNamesInput, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
        }


        public string GetCurrentVoteCount(bool isRestCall, string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, bool alphabeticalSort = true, bool simple = true, bool lSort = true, bool cleanDay = false, bool displayAllVCs = false)
        {
            
            VoteServiceObject vso = BuildBaseVoteObject(isRestCall, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
            if (vso.ErrorMessage != null)
            {
                return vso.ErrorMessage;
            }
            else
            {
                try
                {
                    return BuildHistoryLogic.BuildVoteCount(BuildHistoryLogic.BuildHistoryObject(vso, alphabeticalSort, simple, lSort, cleanDay, displayAllVCs));
                }
                catch(Exception e)
                {
                    string host = "";
                    try
                    {
                        host = System.Web.HttpContext.Current.Request.Url.Host;
                    }
                    catch {
                        host = null;
                    }
                    if (host == null) host = "null";

                    Exception innerException = e.InnerException;
                    string innerExceptionMessage = innerException != null ? innerException.StackTrace : "notApplicable";

                    string message = e.Message != null ? e.Message : " no message found";


                    return "An error occurred while processing the votes. Please check input data or give MathBlade a PM with the thread and post number.";// + "MESSAGE: " + message + "INNER EXCEPTION: " + innerExceptionMessage + "STACK TRACE: " + e.StackTrace + "url=" + host;

                }
            }

        }

        public string GetCurrentVoteCountREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, string alphabeticalSort, string simple, string lSort, string cleanDay, string displayAllVCs)
        {
            url = System.Web.HttpUtility.UrlDecode(url);
            playerList = System.Web.HttpUtility.UrlDecode(playerList);
            
            if (!optionalVoteOverrideData.Equals("null"))
            {
                optionalVoteOverrideData = System.Web.HttpUtility.UrlDecode(optionalVoteOverrideData);
            }
            else
            {
                optionalVoteOverrideData = null;
            }
            optionalFlavorText = optionalFlavorText.Equals("null") ? null : optionalFlavorText;
            optionalDeadlineText = optionalDeadlineText.Equals("null") ? null : optionalDeadlineText;
            return GetCurrentVoteCount(true, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData, Boolean.Parse(alphabeticalSort), Boolean.Parse(simple), Boolean.Parse(lSort), Boolean.Parse(cleanDay), Boolean.Parse(displayAllVCs));
        }

        public string GetASpecificWagonHistory(bool isRestCall, string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, bool alphabeticalSort = true, bool simple = true, bool lSort = true)
        {
            
            VoteServiceObject vso = BuildBaseVoteObject(isRestCall, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
            if (vso.ErrorMessage != null)
            {
                return vso.ErrorMessage;
            }
            else
            {
                try
                {
                    HistoryServiceObject hso = BuildHistoryLogic.BuildHistoryObject(vso, alphabeticalSort, simple, lSort);
                    return BuildHistoryLogic.BuildWagonData(hso);
                }
                catch
                {
                    return "An error occurred while building the wagon data. Please call 'GetVotes' with the same data so MathBlade can find the issue. ";
                }
            }

        }

        public string GetASpecificWagonHistoryREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, string alphabeticalSort, string simple, string lSort)
        {
            url = System.Web.HttpUtility.UrlDecode(url);
            playerList = System.Web.HttpUtility.UrlDecode(playerList);
            if (!optionalVoteOverrideData.Equals("null"))
            {
                optionalVoteOverrideData = System.Web.HttpUtility.UrlDecode(optionalVoteOverrideData);
            }
            else
            {
                optionalVoteOverrideData = null;
            }
            optionalFlavorText = optionalFlavorText.Equals("null") ? null : optionalFlavorText;
            optionalDeadlineText = optionalDeadlineText.Equals("null") ? null : optionalDeadlineText;

            return GetASpecificWagonHistory(true, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData, Boolean.Parse(alphabeticalSort), Boolean.Parse(simple), Boolean.Parse(lSort));
        }

        public string GetAllHistories(bool isRestCall, string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData)
        {

            VoteServiceObject vso = BuildBaseVoteObject(isRestCall, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
            if (vso.ErrorMessage != null)
            {
                return vso.ErrorMessage;
            }
            else
            {
                try
                {
                    HistoryServiceObject hso = BuildHistoryLogic.BuildHistoryObject(vso);
                    return BuildHistoryLogic.BuildWagonDataOfAllTypes(vso);
                }
                catch
                {
                    return "An error occurred while building the wagon data. Please call 'GetVotes' with the same data so MathBlade can find the issue. ";
                }
            }

        }

        public string GetAllHistoriesREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData)
        {
            url = System.Web.HttpUtility.UrlDecode(url);
            playerList = System.Web.HttpUtility.UrlDecode(playerList);
            if (!optionalVoteOverrideData.Equals("null"))
            {
                optionalVoteOverrideData = System.Web.HttpUtility.UrlDecode(optionalVoteOverrideData);
            }
            else
            {
                optionalVoteOverrideData = null;
            }
            optionalFlavorText = optionalFlavorText.Equals("null") ? null : optionalFlavorText;
            optionalDeadlineText = optionalDeadlineText.Equals("null") ? null : optionalDeadlineText;

            return GetAllHistories(true, url, playerList, replacementList, moderatorNames, dayStartList, deadListText, priorVCNumberInput, optionalFlavorText, optionalDeadlineText, optionalVoteOverrideData);
        }

        private VoteServiceObject BuildBaseVoteObject(bool isRestCall, string url, string playerList, string replacementList, string moderatorNamesInput, string dayStartList = null, string deadListText = null, string priorVCNumberInput = null, string optionalFlavorText = null, string optionalDeadlineText = null, string optionalVoteOverrideData = null)
        {
            try
            {
                urlOfGame = url;
                playerText = playerList;
                replacementText = replacementList;
                moderatorNamesText = moderatorNamesInput;
                dayPostStartList = dayStartList;
                deadPlayerListText = deadListText;
                priorVCNumberInputText = priorVCNumberInput;
                flavorText = optionalFlavorText;
                deadlineCode = optionalDeadlineText;
                voteOverrideText = optionalVoteOverrideData;

                string errorsInGivenData = validateAllUserInputs();

                if (errorsInGivenData != null)
                {
                    return new VoteServiceObject(null, "Invalid inputs provided. Ending processing. Reason: " + errorsInGivenData);
                }


                if (validateURL(url) == true)
                {

                    votesByOverride = Day.parseVoteText(players, voteOverrideText);



                    return new VoteServiceObject(urlOfGame, players, replacements, moderatorNames, dayStartPostNumbers, nightkilledPlayers, priorVCNumber, flavorText, deadlineCode, votesByOverride, isRestCall);
                }
                else {
                    System.Console.WriteLine("Invalid URL given in. Ending processing.");

                    return new VoteServiceObject(null, "Invalid URL given in. Ending processing.");
                }
            }
            catch (Exception e)
            {
                return new VoteServiceObject(null, "InputData: " + GetVotesDebug(urlOfGame,playerList,replacementList,moderatorNamesInput,dayStartList,deadListText,priorVCNumberInput,optionalFlavorText,optionalDeadlineText,optionalVoteOverrideData) + " An unexpected error occurred. Please PM MathBlade with input data or temporarily hand count votes." + " MESSAGE: " + (e.Message != null ? e.Message : "No message" ) + " STACK TRACE: " + e.StackTrace);
            }
        }
        private string validateAllUserInputs()
        {
            string errorList = null;
            try
            {
                
                errorList = doErrorCheck(validatePlayerList, errorList);
                errorList = doErrorCheck(validateReplacementsList, errorList);
                errorList = doErrorCheck(validateDayPostNumbers, errorList);
                errorList = doErrorCheck(validateDeaths, errorList);
                errorList = doErrorCheck(validateModeratorNames, errorList);
                errorList = doErrorCheck(validatePriorVCNumber, errorList);
                errorList = doErrorCheck(validateFlavorText, errorList);
                errorList = doErrorCheck(validateDeadline, errorList);
                errorList = doErrorCheck(validateVoteOverrideData, errorList);
            }
            catch (Exception e)
            {
                return "An error occured in validating the data: " + e.StackTrace;
            }


            return errorList;

        }

        private string doErrorCheck(Func<string> functionName, string priorErrors)
        {


            string errors = functionName();
            if (errors != null)
            {
                return errors;
            }
            if (priorErrors != null)
            {
                if (errors != null)
                {
                    return priorErrors + "," + errors;
                }
                else
                {
                    return priorErrors;
                }
            }
            return null;
        }

        private string validateText(string text)
        {

            if (text != null && text.Length > 0)
            {
                text = text.Trim();
                return text;
            }
            else {
                return null;
            }
        }

        private string validatePlayerList()
        {
            if (this.playerText == null)
            {
                return "You need to provide a comma separated list of players.";
            }
            string playerText = validateText(this.playerText);


            string[] playerCommaSplitList = playerText.Split(COMMA);
            players = new List<Player>();

            foreach (string playerName in playerCommaSplitList)
            {
                try
                {
                    players.Add(new Player(playerName.Trim()));
                }
                catch
                {
                    return "Player Name: " + playerName + " was not formatted correctly.";
                }
            }

            if ((players.Count == 0) || (players.Count == 1))
            {

                System.Console.WriteLine("ERROR: You must provide a comma delimited list of players.");
                return "ERROR: You must provide a comma delimited list of players.";
            }



            return null;

        }

        private string validateReplacementsList()
        {
            try
            {
                replacements = new List<Replacement>();
                if (replacementText == null || replacementText.Length == 0)
                {
                    return null;
                }

                string[] replacementsCommaSplitList = replacementText.Split(COMMA);

                string[] replacementArray = null;
                
                Replacement replacement = null;
                foreach (string replacementGroup in replacementsCommaSplitList)
                {
                    if (replacementGroup.Length > 0)
                    {
                        replacementArray = replacementGroup.Split(COLON);
                        replacement = new Replacement(replacementArray[0].Trim(), replacementArray[1].Trim());
                        replacements.Add(replacement);
                    }

                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("ERROR: There was a problem with the replacement list. Please check your format and try again. ");
                return "ERROR: There was a problem with the replacement list. Please check your format and try again. " + e.StackTrace;
            }

            return null;
        }

        private string validateDayPostNumbers()
        {
            dayStartPostNumbers = new List<int>();
            if (dayPostStartList == null)
            {
                dayStartPostNumbers.Add(0);
                return null;
            }
            string dayStartPostNumberText = validateText(dayPostStartList);
            dayStartPostNumbers = new List<int>();
            if (dayStartPostNumberText == null)
            {
                dayStartPostNumbers.Add(0);
                return null;
            }
            string[] postNumberList = dayStartPostNumberText.Split(COMMA);


            int lastNewPostNumber = -2;
            bool isValidList = true;
            foreach (string aPostNumberString in postNumberList)
            {
                int newPostNumber = -1;
                bool isANumber = int.TryParse(aPostNumberString.Trim(), out newPostNumber);
                if (isANumber && newPostNumber > -1 && newPostNumber > lastNewPostNumber)
                {
                    dayStartPostNumbers.Add(newPostNumber);
                    lastNewPostNumber = newPostNumber;
                }
                else
                {
                    isValidList = false;
                    break;
                }
            }

            if (!isValidList)
            {

                System.Console.WriteLine("ERROR: Check the input of your post day start numbers;");
                return ("ERROR: Check the input of your post day start numbers; ");
            }
            else if (dayStartPostNumbers.Count == 0)
            {
                dayStartPostNumbers.Add(0);
            }

            return null;
            
        }

        private string validateDeaths()
        {
            if (deadPlayerListText == null || deadPlayerListText.Length == 0)
            {
                nightkilledPlayers = new List<List<Player>>();
                return null;
            }

            string[] deadTextSplit = this.deadPlayerListText.Split(Convert.ToChar(","));

            //List<Player> deadPlayers = new List<Player> ();
            string[] deadPlayerStringSplit = null;
            string playerName = null;
            int nightOfDeath = -1;
            nightkilledPlayers = new List<List<Player>>();

            //List<List<Vote>> votesByDay = new List<List<Vote>> ();

            if (deadTextSplit.Length > 0)
            {
                foreach (string deadPlayerString in deadTextSplit)
                {
                    if (deadPlayerString.Length > 0)
                    {
                        deadPlayerStringSplit = deadPlayerString.Split(Convert.ToChar("-"));
                        playerName = deadPlayerStringSplit[0].Trim();
                        nightOfDeath = Int32.Parse(deadPlayerStringSplit[1].Trim());

                        while (nightkilledPlayers.Count < (nightOfDeath + 1))
                        {
                            nightkilledPlayers.Add(new List<Player>());
                        }

                        Player playerKilled = Player.FindPlayerByNameUserAid(players, playerName);
                        if (playerKilled != null)
                        {
                            nightkilledPlayers[nightOfDeath - 1].Add(playerKilled);

                        }
                        else {

                            playerKilled = Player.FindPlayerByNameUserAidReplacementsLoop(players, playerName, replacements);
                            if (playerKilled != null)
                            {
                                nightkilledPlayers[nightOfDeath - 1].Add(playerKilled);
                            }
                            else {
                                System.Console.WriteLine("Error finding player killed. Please check spelling for players killed. Bad input: " + playerName);
                                return ("Error finding player killed. Please check spelling for players killed. Bad input: " + playerName);
                            }
                        }



                    }
                }
            }


            return null;
        }

        private string validateModeratorNames()
        {


            string moderatorNamesString = validateText(moderatorNamesText);



            if (moderatorNamesString == null)
            {
                System.Console.WriteLine("No moderator specified. This is a required field.");
                return ("No moderator specified. This is a required field.");
            }

            string[] moderatorNamesArray = moderatorNamesString.Split(COMMA);
            moderatorNames = new List<string>();
            foreach (string name in moderatorNamesArray)
            {
                moderatorNames.Add(name);
            }

            if (moderatorNames.Count == 0)
            {
                System.Console.WriteLine("No moderator specified. This is a required field.");
                return ("No moderator specified. This is a required field.");
            }

            return null;
        }


        private string validatePriorVCNumber()
        {
            string vcNumberText = this.priorVCNumberInputText;
            if (vcNumberText == null || vcNumberText.Length == 0)
            {
                priorVCNumber = 0;
                return null;
            }

            return (int.TryParse(vcNumberText, out priorVCNumber) ? null : "Error you did not provide a number for prior vote count number.");
        }


        private string validateFlavorText()
        {

            if (flavorText == null || flavorText.Length == 0)
            {
                flavorText = "This is an automated vote count generated by a tool written by MathBlade. It goes much smoother with exact votes but will try to detect bold votes and misspellings. If you have issues during this beta, please get MathBlade.";
            }

            return null;
        }

        private string validateDeadline()
        {

            if (deadlineCode == null || deadlineCode.Length == 0)
            {
                deadlineCode = "(FIX ME)";
            }

            return null;
        }

        private string validateVoteOverrideData()
        {

            if (voteOverrideText == null || voteOverrideText.Length == 0)
            {
                return null;
            }

            try
            {
                votesByOverride = Day.parseVoteText(players, voteOverrideText);
            }

            catch (Exception e)
            {
                System.Console.WriteLine("Error occurred in processing override. Check format. Exception: " + e.StackTrace);
                return ("||" + voteOverrideText + "|| " + " Error occurred in processing override. Check format. Exception: " + e.StackTrace);
            }

            return null;
        }




        private bool validateURL(string url)
        {
            if ((url == null) || (url.Length == 0))
            {
                return false;
            }
            return true;
            //Uri uriResult;
            //bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
               // && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps) && uriResult != null;
            //return result;
        }

        private static string getRequiredValue(string prompt, List<string> promptsUsed, List<string> values)
        {

            int indexOfRequiredItem = promptsUsed.IndexOf(prompt);
            if (indexOfRequiredItem == -1)
            {
                throw new ArgumentNullException();
            }

            return values[indexOfRequiredItem].Replace("<br>","");
        }

        private static string getOptionalValue(string prompt, string defaultIfMissing, List<string> promptsUsed, List<string> values)
        {
            int indexOfRequiredItem = promptsUsed.IndexOf(prompt);
            if (indexOfRequiredItem == -1)
            {
                return defaultIfMissing;
            }

            return values[indexOfRequiredItem].Replace("<br>","");
        }

        private static bool promptLoop(ref string remainingString, ref List<string> prompts, ref List<string> usedPrompts, ref List<string> values)
        {

            foreach (string prompt in prompts)
            {
                if (remainingString.StartsWith(prompt))
                {
                    prompts.Remove(prompt);
                    usedPrompts.Add(prompt);
                    string currentPrompt = prompt;
                    remainingString = remainingString.Substring(prompt.Length);
                    List<int> indexesOfOtherPrompts = new List<int>();
                    foreach (string differentPrompt in prompts)
                    {
                        int index = remainingString.IndexOf(differentPrompt);
                        if (index > -1)
                        {
                            indexesOfOtherPrompts.Add(index);

                        }
                        else
                        {
                            indexesOfOtherPrompts.Add(int.MaxValue);
                        }
                    }

                    string value;
                    if (indexesOfOtherPrompts.Count > 0)
                    {
                        int minIndex = indexesOfOtherPrompts.Min();

                        if (minIndex > -1 && minIndex != int.MaxValue)
                        {
                            value = remainingString.Substring(0, minIndex);
                            values.Add(value);
                            remainingString = remainingString.Substring(minIndex);
                            return true;
                        }
                        else
                        {
                            value = remainingString;
                            values.Add(value);
                            return false;
                        }
                    }
                    else
                    {
                        value = remainingString;
                        values.Add(value);
                        return false;
                    }



                }
            }

            return false;
        }


        private VoteScrubInformationObject ScrubInnerText(bool isRestCall, string innerText)
        {
            List<string> promptsFound = new List<string>();
            List<string> valuesFound = new List<string>();

            string remainingString = innerText;

            List<string> promptsLeft = new List<string>();
            promptsLeft.AddRange(ALL_PROMPTS);


            bool workLeftToDo = true;
            List<string> usedPrompts = new List<string>();
            List<string> values = new List<string>();
            string originalText = (string)innerText.Clone();

            
                while (workLeftToDo == true)
                {
                    workLeftToDo = promptLoop(ref innerText, ref promptsLeft, ref usedPrompts, ref values);
                }

                string priorVCNumberInput = getRequiredValue(VOTE_NUMBER_INPUT, usedPrompts, values);
                string urlOfGame = System.Web.HttpUtility.HtmlDecode(getRequiredValue(URL_OF_GAME_PROMPT, usedPrompts, values)).Replace("&amp;", "&");
                string playerTextInput = getRequiredValue(PLAYER_TEXT_PROMPT, usedPrompts, values);
                string replacementTextInput = getOptionalValue(REPLACEMENTS_LIST_PROMPT, null, usedPrompts, values);
                string moderatorNamesInput = getRequiredValue(MOD_LIST_PROMPT, usedPrompts, values);
                string dayNumbersInput = getOptionalValue(DAY_NUMBERS_PROMPT, null, usedPrompts, values);
                string deadListInput = getOptionalValue(DEAD_LIST_PROMPT, null, usedPrompts, values);
                string deadLineInput = getOptionalValue(DEADLINE_PROMPT, null, usedPrompts, values);
                string flavorInput = getOptionalValue(FLAVOR_PROMPT, null, usedPrompts, values);
                string voteOverridesInput = getOptionalValue(VOTE_OVERRIDES_PROMPT, null, usedPrompts, values);
                string alphaSortInput = getOptionalValue(ALPHA_SORT_PROMPT, "true", usedPrompts, values);
                string simpleInput = getOptionalValue(SIMPLE_SORT_PROMPT, "true", usedPrompts, values);
                string lSortInput = getOptionalValue(L_SORT_PROMPT, "true", usedPrompts, values);
                string cleanDayInput = getOptionalValue(CLEAN_DAY_PROMPT, "false", usedPrompts, values);
                string displayAllVCsInput = getOptionalValue(DISPLAY_ALL_VCs_PROMPT, "false", usedPrompts, values);

                if (voteOverridesInput != null)
                {
                    try
                    {
                        HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
                        htmlDocument.LoadHtml(voteOverridesInput);
                        voteOverridesInput = System.Web.HttpUtility.HtmlDecode(htmlDocument.DocumentNode.SelectSingleNode(".//code").InnerText);
                    }
                    catch
                    {
                        voteOverridesInput = "";

                    }
                }

                return new VoteScrubInformationObject(priorVCNumberInput, urlOfGame, playerTextInput, dayNumbersInput, replacementTextInput, moderatorNamesInput, deadListInput, deadLineInput, flavorInput, voteOverridesInput, alphaSortInput, simpleInput, lSortInput, cleanDayInput, displayAllVCsInput);
            
            
        }

        private string GetHistoriesFromSettingsString(bool isRestCall, string innerText, bool allHistories)
        {
            try
            {
                VoteScrubInformationObject vsio = ScrubInnerText(isRestCall, innerText);
                if (allHistories == true)
                {
                    return GetAllHistories(isRestCall, vsio.UrlOfGame, vsio.PlayerTextInput, vsio.ReplacementTextInput, vsio.ModeratorNamesInput, vsio.DayNumbersInput, vsio.DeadListInput, vsio.PriorVCNumberInput, vsio.FlavorInput, vsio.DeadLineInput, vsio.VoteOverridesInput);
                }
                else
                {
                    return GetASpecificWagonHistory(isRestCall, vsio.UrlOfGame, vsio.PlayerTextInput, vsio.ReplacementTextInput, vsio.ModeratorNamesInput, vsio.DayNumbersInput, vsio.DeadListInput, vsio.PriorVCNumberInput, vsio.FlavorInput, vsio.DeadLineInput, vsio.VoteOverridesInput);
                }
            }
            catch 
            {
                return "Could not scrub settings post. Please verify data and try again.";
            }
        }


        private string GetVoteJSONFromSettingsString(bool isRestCall, string innerText, string playerName)
        {
            try
            {
                VoteScrubInformationObject vsio = ScrubInnerText(isRestCall, innerText);
                if (string.IsNullOrEmpty(playerName))
                {
                    return GetVotes(isRestCall, vsio.UrlOfGame, vsio.PlayerTextInput, vsio.ReplacementTextInput, vsio.ModeratorNamesInput, vsio.DayNumbersInput, vsio.DeadListInput, vsio.PriorVCNumberInput, vsio.FlavorInput, vsio.DeadLineInput, vsio.VoteOverridesInput);
                }
                else
                {
                    return GetVotes(isRestCall, vsio.UrlOfGame, vsio.PlayerTextInput, vsio.ReplacementTextInput, vsio.ModeratorNamesInput, vsio.DayNumbersInput, vsio.DeadListInput, vsio.PriorVCNumberInput, vsio.FlavorInput, vsio.DeadLineInput, vsio.VoteOverridesInput, playerName);
                }
            }
            catch (Exception e)
            {
                return "Could not scrub settings post. Please verify data and try again.";
            }
        }

        private string GetModCountFromSettingsString(bool isRestCall, string innerText)
        {
            
            try
            {
                VoteScrubInformationObject vsio = ScrubInnerText(isRestCall, innerText);
                return GetCurrentVoteCount(isRestCall, vsio.UrlOfGame, vsio.PlayerTextInput, vsio.ReplacementTextInput, vsio.ModeratorNamesInput, vsio.DayNumbersInput, vsio.DeadListInput, vsio.PriorVCNumberInput, vsio.FlavorInput, vsio.DeadLineInput, vsio.VoteOverridesInput, Boolean.Parse(vsio.AlphaSortInput), Boolean.Parse(vsio.SimpleInput), Boolean.Parse(vsio.LSortInput), Boolean.Parse(vsio.CleanDayInput), Boolean.Parse(vsio.DisplayAllVCsInput));
            }
            catch 
            {
                return "Could not scrub settings post. Please verify data and try again.";
            }

              
        }



    }

    
}
