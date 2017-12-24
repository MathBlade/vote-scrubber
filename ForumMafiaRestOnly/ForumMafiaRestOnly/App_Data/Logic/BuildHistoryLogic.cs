using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public static class BuildHistoryLogic
{
    public static HistoryServiceObject BuildHistoryObject(VoteServiceObject vso)
    {
        return new HistoryServiceObject(vso, false, false, false);
    }
    public static HistoryServiceObject BuildHistoryObject(VoteServiceObject vso, bool sortBy, bool simple, bool lSort)
    {
        return new HistoryServiceObject(vso, sortBy, simple, lSort, false, false);
    }

    public static HistoryServiceObject BuildHistoryObject(VoteServiceObject vso, bool sortBy, bool simple, bool lSort, bool cleanDay, bool displayAllVCs)
    {

        return new HistoryServiceObject(vso, sortBy, simple, lSort, cleanDay, displayAllVCs);


    }

    public static string BuildVoteCount(HistoryServiceObject hso)
    {
        string returnString = "";
        ResultsObject ro = null;
        try
        {
            ro = BuildOneResultsObject(hso);
                       
            if (hso.DisplayAllVCs)
            {
                foreach (VoteCount vc in ro.AllVCs)
                {
                    returnString += vc.LatestVCOutput; //vc.buildLatest (vc.DayNumber, priorVCNumber, flavorText, deadlineCode, days[vc.DayNumber - 1].DeathsOvernight.Count) + History.NEW_LINE_HERE;
                }
            }
            else
            {
                returnString = ro.FinalVoteCount.LatestVCOutput;
            }

        }
        catch (Exception e)
        {
            return "History Report failed: " + "Report object: " + ((ro == null) ? "null" : "not null" + ("FinalVoteCount: " + ((ro.FinalVoteCount == null) ? "null" : "not null"))) + "INNER EXCEPTION: " + e.InnerException + "MESSAGE: " +  e.Message + " STACK TRACE: " + e.StackTrace;
        }

        //Add timings
        List<long> millisecondCallList = hso.VSO.MillisecondsEachCall;

        if (millisecondCallList != null)
        {

            returnString += ((hso.VSO.IsRestCall) ? "<br/><br/>" : "") + "Performed " + ((long) hso.VSO.MillisecondsEachCall.Count) + " calls in " + ((long) millisecondCallList.Sum() / (long) 1000) + " seconds. With an average of " + (millisecondCallList.Average() / (long)1000) + " seconds per call. ";
            
           
          
        }
    

        return returnString;
    }
    
    public static string BuildWagonDataOfAllTypes(VoteServiceObject vso)
    {
        string allWagonData = "";
        VoteServiceObject clonedVSO = vso.DeepClone<VoteServiceObject>();
        int settingsCount = 3;
        List<bool> defaultVCSettings = new List<bool>();
        defaultVCSettings.Add(true);
        defaultVCSettings.Add(false);
        defaultVCSettings.Add(true);

        

        ResultsObject defaultResultObject = null;

        List<ResultsObject> resultsObjects = new List<ResultsObject>();

        List<HistoryServiceObject> hsosToParse = new List<HistoryServiceObject>();
        for (int i = 0; i < Math.Pow(2, settingsCount); i++)
        {
            BitArray b = new BitArray(new int[] { i });
            IEnumerable<bool> boolArray = b.Cast<bool>().Select(bit => bit ? true : false).ToArray().Take(settingsCount);
            HistoryServiceObject hso = new HistoryServiceObject(clonedVSO, boolArray.ElementAt(0), boolArray.ElementAt(1), boolArray.ElementAt(2));
            
            resultsObjects.Add(BuildHistoryLogic.BuildOneResultsObject(hso));
            clonedVSO = vso.DeepClone<VoteServiceObject>();

            if (defaultResultObject == null)
            {
                if (boolArray.SequenceEqual(defaultVCSettings))
                {
                    defaultResultObject = resultsObjects[i];
                }
            }

            allWagonData += resultsObjects[i].WagonText;
        }

        return allWagonData;


    }

    public static string BuildWagonData(HistoryServiceObject hso)
    {
        ResultsObject ro = BuildOneResultsObject(hso);
        return ro.WagonText;
    }

    public static List<List<Vote>> BuildVotesByDay(List<Vote> votesOnThread, List<int> dayStartPostNumbers)
    {
        

        int maxPostNumberOfDay = int.MaxValue;
        List<List<Vote>> votesByDay = new List<List<Vote>>();
        for (int i = 0; i < dayStartPostNumbers.Count; i++)
        {
            int postMin = dayStartPostNumbers[i];
            if (dayStartPostNumbers.Count > (i + 1))
            {
                maxPostNumberOfDay = dayStartPostNumbers[i + 1];
            }
            else if (dayStartPostNumbers.Count == (i + 1))
            {
                maxPostNumberOfDay = int.MaxValue;
            }

            if (votesOnThread != null)
            {

                foreach (Vote voteInThread in votesOnThread)
                {
                    if ((voteInThread.PostNumber > postMin && voteInThread.PostNumber < maxPostNumberOfDay))
                    {
                        while (votesByDay.Count < (i + 1))
                        {
                            votesByDay.Add(new List<Vote>());
                        }
                        voteInThread.DayNumber = i + 1;
                        votesByDay[i].Add(voteInThread);
                    }
                }



            }

            if ((i == (dayStartPostNumbers.Count - 1)))
            {
                if (votesByDay.Count < i + 1)
                {
                    votesByDay.Add(new List<Vote>());
                }
            }
        }

        if (votesByDay.Count == 0 && votesOnThread.Count > 0)
        {
            throw new SystemException("Votes by day count is zero. Votes found: " + votesOnThread.Count + " dayStartPostNumbers: " + (dayStartPostNumbers != null ? string.Join(",", dayStartPostNumbers) : " null "));
        }

        return votesByDay;

    }

    public static List<Day> BuildDays(List<List<Vote>> votesByDay, List<List<Player>> nightkilledPlayers, List<Player> allPlayers)
    {
        List<Day> days = new List<Day>();
        if (votesByDay.Count == 0)
        {
            throw new SystemException("No votes found");
        }
        for (int j = 0; j < votesByDay.Count; j++)
        {
            days.Add(new Day(j + 1, (nightkilledPlayers.Count > j) ? nightkilledPlayers[j] : new List<Player>(), allPlayers, votesByDay[j]));

        }

        return days;
    }



    private static ResultsObject BuildOneResultsObject(HistoryServiceObject hso)//(List<Day> days, List<Player> allPlayers, bool sortBy, bool simple, bool lSort, int priorVCNumber, string flavorText, string deadlineCode, bool cleanDay)
    {

        return createWagonData(hso.Days, hso.AllPlayers, hso.SortBy, hso.Simple, hso.LSort, hso.PriorVCNumber, hso.FlavorText,hso.DeadlineCode,hso.CleanDay, hso.IsRestCall);
    }

    private static ResultsObject createWagonData(List<Day> days, List<Player> allPlayers, bool sortBy, bool simple, bool lSort, int priorVCNumber, string flavorText, string deadlineCode, bool cleanDay, bool isRestCall)
    {
        VoteCount vc = null;
        History history = null;
        List<VoteCount> allVCs = new List<VoteCount>();
        string displayText = "[area=WAGONS(Sort By: " + (sortBy ? "Alphabetical" : "Chronological") + " Data Type: " + (simple ? "Simple" : "Complex") + " LSort: " + (lSort ? "On" : "Off") + ") ] Note from vote counter. These votes include any vote on or off that impacted said wagon.";

        if (days.Count == 0)
            throw new System.Exception("No days found.");

        for (int i = 0; i < days.Count; i++)
        {


            history = new History(lSort, isRestCall);
            days[i].Votes.Sort();

            System.Console.WriteLine("Players In Game: " + allPlayers.Count + " PlayersAlive(): " +  allPlayers.numAlive() + "Threshold: " + ((int)(Math.Floor((double) allPlayers.numAlive() / 2)) + 1));
            vc = new VoteCount(days[i].Number, allPlayers, (((int)Math.Floor((double) allPlayers.numAlive() / 2)) + 1), days, isRestCall);




            foreach (Vote vote in days[i].Votes)
            {
                vc.doVote(vote, vc.MaxThreshold, lSort, sortBy);
                history.update(vc);
                if (vc.IsHammered)
                {

                    break;
                }
            }

            vc.buildLatest(vc.DayNumber, priorVCNumber, flavorText, deadlineCode, (days[vc.DayNumber - 1].DeathsOvernight) != null ? days[vc.DayNumber - 1].DeathsOvernight.Count : 0);

            days[i].performDeathsOvernight();
            foreach (Player player in allPlayers)
            {
                foreach (Player deadPlayer in days[i].DeathsOvernight)
                {
                    if (deadPlayer.Name.Equals(player.Name))
                    {
                        player.IsAlive = false;
                        player.IsDead = true;
                    }
                }
            }




            allVCs.Add(vc);

            displayText = displayText + history.buildDisplayString(days[i].Number, simple);


        }

        displayText = displayText + "[/area]";

        if (allVCs == null)
        {
            throw new ArgumentNullException("NO VCs generated");
        }

        if (displayText == null)
        {
            throw new ArgumentNullException("No display Text");
        }
       

        return new ResultsObject(allVCs, displayText, cleanDay);


    }


}
