using System;
using System.Collections.Generic;
using System.Diagnostics;

public class HistoryServiceObject
{

    List<List<Vote>> votesByDay;
    List<Day> days;
    VoteServiceObject vso;
    bool sortBy;
    bool simple;
    bool lSort;
    bool cleanDay;
    bool displayAllVCs;
    Stopwatch stopWatch;
   

    public HistoryServiceObject(VoteServiceObject _vso, bool _sortBy, bool _simple, bool _lSort):this( _vso, _sortBy, _simple, _lSort, false, false)
    {

    }

    public HistoryServiceObject(VoteServiceObject _vso, bool _sortBy, bool _simple, bool _lSort, bool _cleanDay, bool _displayAllVCs)
    {
        vso = _vso;
        votesByDay = BuildHistoryLogic.BuildVotesByDay(vso.Votes, vso.DayStartPostNumbers);
        days = BuildHistoryLogic.BuildDays(votesByDay, vso.NightkilledPlayers, vso.Players);
        sortBy = _sortBy;
        simple = _simple;
        lSort = _lSort;
        cleanDay = _cleanDay;
        displayAllVCs = _displayAllVCs;
        
        
    }

   
    public bool IsRestCall {  get { return vso.IsRestCall; } }
    public List<List<Vote>> VotesByDay { get { return votesByDay; } }
    public List<Day> Days { get { return days; } }
    public VoteServiceObject  VSO { get { return vso; } }
    public List<Player> AllPlayers {  get { return VSO.Players;  } }
    public bool SortBy {  get { return sortBy;  } }
    public bool Simple { get { return simple; } }
    public bool LSort { get { return lSort; } }
    public int PriorVCNumber {  get { return VSO.PriorVCNumber; } }
    public string FlavorText {  get { return VSO.FlavorText;  } }
    public string DeadlineCode { get { return VSO.DeadlineCode; } }
    public bool CleanDay {  get { return cleanDay; } }
    public bool DisplayAllVCs {  get { return displayAllVCs; } }



}
