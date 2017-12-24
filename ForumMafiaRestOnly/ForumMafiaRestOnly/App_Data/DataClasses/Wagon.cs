using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class Wagon :IComparable<Wagon> {

	Player playerBeingVoted;
	List<Player> playersVoting;
	int maxThreshold;

	bool isHammered;
	List<int> postNumbers;
	List<string> postBBCodeList;
	List<bool> unvoteList;
	DateTime maxTimeStamp;
	List<Vote> allVotesRelevantToWagon;
	bool lSort;
	bool alphaSort;

    public const string NO_LYNCH = "No Lynch";
    public static readonly Player NO_LYNCH_PLAYER = new Player(NO_LYNCH);

	public Wagon(Player _playerBeingVoted, Player _firstPlayerToVote, int _postNumber, string _postBBCode, int _maxThreshold, DateTime _timestamp, Vote _firstVote, bool _lSort, bool _alphaSort)
	{
		playerBeingVoted = _playerBeingVoted;
		playersVoting = new List<Player> ();
		_firstPlayerToVote.PlayerCurrentlyVoting = _playerBeingVoted;
		playersVoting.Add (_firstPlayerToVote);


		postNumbers = new List<int> ();
		postNumbers.Add ( _postNumber);

		postBBCodeList = new List<string> ();
		postBBCodeList.Add (_postBBCode);

		unvoteList = new List<bool> ();
		unvoteList.Add (false);

		maxThreshold = _maxThreshold;
		isHammered = false;

		maxTimeStamp = _timestamp;

		allVotesRelevantToWagon = new List<Vote> ();
		allVotesRelevantToWagon.Add (_firstVote);


		lSort = _lSort;
		alphaSort = _alphaSort;
		//l_Level = maxThreshold - 1;
	}

	public string ToHistoricalDisplayString(bool complexOn)
	{
		string complexOffString = " " + GetDisplayStringForSimplePostCount ();
		string complexOnString = " [ " + voteBBCountsRelevantToThisWagon () + " ]";
		string wagonDataString = complexOn ? complexOnString : complexOffString;

		return "[b] [color=blue]" + PlayerBeingVoted.Name + " (" +  playersVoting.Count + ")" +  "[/color][/b] ~ [color=#FF80FF]" + playersVotingCommaDelimitedList() + "[/color]" +  wagonDataString;


	}

	private string GetDisplayStringForSimplePostCount()
	{
		bool lastVoteIsUnvote = unvoteList[(unvoteList.Count -1)];
		int postThatMadeWagon = GetPostThatMadeWagon ();
		bool distinctPosts = (postThatMadeWagon != postNumbers [(unvoteList.Count - 1)]);

		if (postThatMadeWagon > 0)
		{
			
			return postBBCodeList[postNumbers.IndexOf(postThatMadeWagon)] + ((lastVoteIsUnvote && distinctPosts) ? "(" + postBBCodeList[(unvoteList.Count -1)] + ")" : "");
		}
		else {
			return "ERROR -- MATHBLADE FIX THIS!";
		}
	}


	private int GetPostThatMadeWagon()
	{
		string playerUnvoteName = null;
		string lastPlayerOnWagon = playersVoting[playersVoting.Count -1].Name;
		string playerBeingVotedName = playerBeingVoted.Name;
		Vote relevantVote = null;

		for (int i = (unvoteList.Count - 1); i > -1; i--) {
			if (unvoteList [i] != true) {
				
				relevantVote = getVoteByPostNumber (postNumbers [i]);
				playerUnvoteName = relevantVote.PlayerVoting.Name;
				if (playerUnvoteName == lastPlayerOnWagon) {
					return postNumbers [i];
				}
			}

		}

		return -1;
	}

	private Vote getVoteByPostNumber(int postNumber)
	{
		foreach (Vote vote in allVotesRelevantToWagon) {
			if (vote.PostNumber == postNumber) {
				return vote;
			}
		}

		return null;
	}

	public int CompareTo( Wagon other) {
		
		if (lSort) {
			if (L_Level != other.L_Level) {
				return (-1 * L_Level.CompareTo (other.L_Level));
			}
		}

		if (alphaSort) {
			if (!(PlayerBeingVoted.Name.Equals (other.PlayerBeingVoted.Name))) {
				return string.Compare (PlayerBeingVoted.Name, other.PlayerBeingVoted.Name);
			}

			return (maxTimeStamp.CompareTo (other.maxTimeStamp));
		} else {


			if (maxTimeStamp.CompareTo (other.maxTimeStamp) != 0) {
				return (maxTimeStamp.CompareTo (other.maxTimeStamp));
			} else {
				return (GetMaxPostNumber().CompareTo(other.GetMaxPostNumber()));
			}

		}



	}

	public int GetMaxPostNumber()
	{
		if (postNumbers.Count == 0) {
			return -1;
		} else {
			int maxPostNumber = 0;
			foreach (int postNumber in postNumbers) {
				if (postNumber > maxPostNumber) {
					maxPostNumber = postNumber;
				}
			}


			return maxPostNumber;
		}
	}

	public void addVote(Vote vote)
	{
		playersVoting.Add (vote.PlayerVoting);
		vote.PlayerVoting.PlayerCurrentlyVoting = vote.PlayerVoted;
		if (!postNumbers.Contains( vote.PostNumber))
		{
			postNumbers.Add (vote.PostNumber);
			postBBCodeList.Add (vote.PostBBCode);
			unvoteList.Add (false);

			if (vote.Timestamp > maxTimeStamp) {
				maxTimeStamp = vote.Timestamp;
			}
			if (vote != null) {
				allVotesRelevantToWagon.Add (vote);
			}
		}

		if (L_Level == 1 && playerBeingVoted.IsHated) {
			isHammered = true;
		} else if (L_Level == 0 && !playerBeingVoted.IsLoved) {
			isHammered = true;
		} else if (L_Level == -1 && playerBeingVoted.IsLoved)  {
			isHammered = true;
		}
	}

	public void removeVote(Vote vote)
	{
		List<Player> playersNowVoting = new List<Player> ();
		foreach (Player playerVoting in playersVoting)
		{
			
			if (playerVoting.Name.Equals (vote.PlayerVoting.Name)) {
				if (!postNumbers.Contains (vote.PostNumber)) {
					postNumbers.Add (vote.PostNumber);
					postBBCodeList.Add (vote.PostBBCode);
					unvoteList.Add (true);
					if (vote.Timestamp > maxTimeStamp) {
						maxTimeStamp = vote.Timestamp;
					}
					if (vote != null) {
						allVotesRelevantToWagon.Add (vote);
					}
				}


			} else {
				playersNowVoting.Add (playerVoting);

			}
		}
		//l_Level = maxThreshold - playersNowVoting.Count;
		playersVoting = playersNowVoting;
	}

	private string voteCountsRelevantToThisWagon()
	{
		string listOfVotes = "";
		if (postNumbers.Count == 0) {
			return "";
		}
		foreach (int integer in postNumbers) {
			listOfVotes = listOfVotes + integer + ", ";
		}
		int lastCommaSpot = listOfVotes.LastIndexOf (", ");
		return listOfVotes.Substring (0, lastCommaSpot);
	}

	private string voteBBCountsRelevantToThisWagon()
	{
		string listOfVotes = "";
		if (postNumbers.Count == 0) {
			return "";
		}
		string modifiedBBCode = "";
		int indexOf = -1;

		int rightBracketIndex = -1;
		foreach (string bbcode in postBBCodeList) {
			modifiedBBCode = bbcode;
			indexOf = postBBCodeList.IndexOf (bbcode);
			if (unvoteList [indexOf] == true) {
				rightBracketIndex = modifiedBBCode.IndexOf ("]") + 1;
				//Debug.Log ("Post number: " + postNumbers [indexOf]);
				//Debug.Log ("Modified BBCode:|" + modifiedBBCode + "|");
				modifiedBBCode = modifiedBBCode.Substring (0, rightBracketIndex) + "[strike]" + modifiedBBCode.Substring (rightBracketIndex, modifiedBBCode.Length - rightBracketIndex - "[/post]".Length) + "[/strike][/post]";

				//modifiedBBCode = modifiedBBCode.Substring (0, rightBracketIndex) + "[strike]" + modifiedBBCode.Substring (rightBracketIndex, modifiedBBCode.Length - rightBracketIndex - "[/url]".Length) + "[/strike][/url]";
			}
			listOfVotes = listOfVotes + modifiedBBCode + ", ";
		}
		int lastCommaSpot = listOfVotes.LastIndexOf (", ");
		return listOfVotes.Substring (0, lastCommaSpot);
	}

	public string playersVotingCommaDelimitedList()
	{
		string listOfPlayers = "";
		if (playersVoting.Count == 0) {
			return "";
		} 

		foreach (Player player in playersVoting) {
			listOfPlayers = listOfPlayers + player.Name + ", ";
		}

		int lastCommaSpot = listOfPlayers.LastIndexOf (", ");
		return listOfPlayers.Substring (0, lastCommaSpot);
	}

	public override string ToString ()
	{
		return PlayerBeingVoted.Name + " ~ " + playersVotingCommaDelimitedList() + " [ " + voteCountsRelevantToThisWagon() + "]";
	}

	public int L_Level
	{
		get {
			return maxThreshold - playersVoting.Count;
		}
	}

	public bool IsHammered
	{
		get {
			return isHammered;
		}
	}

	public Player PlayerBeingVoted
	{
		get {
			return playerBeingVoted;
		}
	}

	public List<Player> PlayersVoting
	{
		get {
			return playersVoting;
		}
	}

	public DateTime TimeStamp
	{
		get {
			return maxTimeStamp;
		}
		set {
			maxTimeStamp = value;
		}
	}
}
