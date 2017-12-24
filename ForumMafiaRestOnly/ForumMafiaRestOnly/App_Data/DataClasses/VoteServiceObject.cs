using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Diagnostics;

[Serializable]
public class VoteServiceObject
{
    private List<Vote> votes;
    private string errorMessage;
    private List<Player> players;
    private List<Replacement> replacements;
    private List<int> dayStartPostNumbers;
    private List<List<Player>> nightkilledPlayers;

    //Stuff that should have been part of the history if I can get it there.
    private int priorVCNumber;
    private string flavorText;
    private string deadlineCode;
    private bool isRestCall;
    
    List<long> millisecondsEachCall;
    long timeElapsed = long.MinValue;
    List<int> pageNumbers;

    [NonSerialized]
    Stopwatch stopWatch;

    public VoteServiceObject(List<Vote> _votes, string _errorMessage = null)
	{
        votes = _votes;
        errorMessage = _errorMessage;
        isRestCall = false;
	}
    public VoteServiceObject(string _urlOfGame, List<Player> _players, List<Replacement> _replacements, List<string> _moderatorNames, List<int> _dayStartPostNumbers, List<List<Player>> _nightkilledPlayers, int _priorVCNumber, string _flavorText, string _deadlineCode, List<Vote> _votesByOverride, bool _isRestCall)
    {
        //System settings
        isRestCall = _isRestCall;

        //User data input
        votes = new List<Vote>();
        List<Vote> regularVotes = new List<Vote>();
        players = _players;
        replacements = _replacements;
        dayStartPostNumbers = _dayStartPostNumbers;
        nightkilledPlayers = _nightkilledPlayers;
        flavorText = _flavorText;
        deadlineCode = "[countdown]" + _deadlineCode + "[/countdown]";
        priorVCNumber = _priorVCNumber;

        if (isRestCall)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
            millisecondsEachCall = new List<long>();
            pageNumbers = new List<int>();
           
        }

        RunScrubLogic.BuildVotes(ref regularVotes, _urlOfGame, _players, _moderatorNames, _replacements, ref millisecondsEachCall);



        List<Vote> overridesCompleted = new List<Vote>();
        foreach (Vote vote in regularVotes)
        {
            bool voteWasOverriden = false;
            foreach (Vote overrideVote in _votesByOverride)
            {
                if (vote.PostNumber == overrideVote.PostNumber)
                {
                    
                    votes.Add(overrideVote);
                    overridesCompleted.Add(overrideVote);
                    voteWasOverriden = true;
                }

                
            }

            if (!voteWasOverriden)
            {
                votes.Add(vote);
            }
        }

        foreach(Vote vote in _votesByOverride)
        {
            if (!overridesCompleted.Contains(vote))
            {
                votes.Add(vote);
            }
        }

       

    }


    public string VoteStringByPlayer (Player player)
    {
        List<Vote> specificPlayerVotes = new List<Vote>(); 
        foreach (Vote vote in votes)
        {
            try
            {
                if (vote.PlayerVoting.Name.Equals(player.Name))
                {
                    specificPlayerVotes.Add(vote);
                }
            }
            catch (Exception e)
            {
                //Do nothing??
                var dummy = 1;
            }
        }

        List<int> voteInts = new List<int>();
        foreach(Vote vote in specificPlayerVotes)
        {
            voteInts.Add(vote.PostNumber);
        }
        voteInts.Sort();
        return string.Join(",", voteInts.ToArray());
        //return JsonConvert.SerializeObject(specificPlayerVotes);
    }


    public string AllVotesString {  get {

            return JsonConvert.SerializeObject(votes);
         } }

    public List<int> PageNumbersScraped {  get { return pageNumbers; } }
    public long TimeElapsed {  get {

            if (stopWatch != null)
            {
                timeElapsed = stopWatch.ElapsedMilliseconds;
                stopWatch.Stop();
                stopWatch = null;
                return timeElapsed;
            }
            else
            {
                return timeElapsed;
            }

        } }
    public List<long> MillisecondsEachCall {  get { return millisecondsEachCall; } }
    public string ErrorMessage {  get { return errorMessage; } }
    public bool IsRestCall {  get { return isRestCall;  } }
    public List<Vote> Votes { get { return votes; } }
    public List<Player> Players {  get { return players;  } }
    public List<int> DayStartPostNumbers {  get { return dayStartPostNumbers; } }
    public List<List<Player>> NightkilledPlayers {  get { return nightkilledPlayers; } }
    public int PriorVCNumber {  get { return priorVCNumber; } }
    public string FlavorText { get { return flavorText;  } }
    public string DeadlineCode {  get { return deadlineCode; } }
    public List<Replacement> Replacements {  get { return replacements; } }
}
