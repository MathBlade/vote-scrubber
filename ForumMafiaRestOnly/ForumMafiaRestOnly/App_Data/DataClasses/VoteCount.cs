using System.Collections;
using System.Collections.Generic;
using System;

public class VoteCount  {



	int dayNumber;
	List<Wagon> wagons;
	//List<Player> playersNotVoting;
	int maxThreshold;
	bool isHammered;
	List<Player> playersInVoteCount;
	List<Day> days;
	string latestVCOutput;
	bool hasVotes;
    private bool isRestCall;


	public VoteCount(int _dayNumber, List<Player> players, int _maxThreshold, List<Day> _days, bool _isRestCall)
	{
		dayNumber = _dayNumber;
		wagons = new List<Wagon> ();
		//playersNotVoting = new List<Player> ();
		maxThreshold = _maxThreshold;
		isHammered = false;
        isRestCall = _isRestCall;

		foreach(Player player in players)
		{
			if (player.IsAlive) {
				player.PlayerCurrentlyVoting = null;
				//playersNotVoting.Add (player);
			}
		}
		playersInVoteCount = players;
		days = _days;
	}

	public bool HasVotes { get { return hasVotes; } }

	public string LatestVCOutput { get { return latestVCOutput; } }

	public int DayNumber
	{
		get {
			return dayNumber;
		}
	}

	public bool IsHammered
	{
		get {
			return isHammered;
		}
	}

	public int MaxThreshold
	{
		get {
			return maxThreshold;
		}
	}

	public List<Wagon> Wagons
	{
		get {
			return wagons;
		}
	}

	public void debugDumpAllWagons()
	{
		foreach (Wagon wagon in wagons) {
			if (wagon.PlayersVoting.Count > 0) {
				System.Console.WriteLine(wagon.ToString ());
			}
		}
		System.Console.WriteLine ("Players Not Voting: " + getPlayersNotVotingString());
	}

	private string getPlayersNotVotingString()
	{
		string listOfPlayers = "";
		/*if (playersNotVoting.Count == 0) {
			return "";
		}
		foreach (Player player in playersNotVoting) {
			listOfPlayers = listOfPlayers + player.Name + ", ";
		}*/

		foreach (Player player in playersInVoteCount) {
			if ((player.PlayerCurrentlyVoting == null) && (player.IsAlive)) {
				listOfPlayers = listOfPlayers + player.Name + ", ";
			}
			
		}

		int lastCommaSpot = listOfPlayers.LastIndexOf (", ");
		if (lastCommaSpot < 0)
			return listOfPlayers;
		else 
			return listOfPlayers.Substring (0, lastCommaSpot);
	}

	public void doVote(Vote vote, int maxThreshold, bool lSort, bool sortBy)
	{
		if (vote != null && !hasVotes) {
			hasVotes = true;
		}

		Player playerTarget = null;
		try 
		{
			Player playerVoting = vote.PlayerVoting;
			//Debug.Log ("VOTE NUMBER: " + vote.PostNumber + " PLAYER VOTING NAME:|" + playerVoting.Name + "|WHERE VOTE IS AT:|" + ((playerVoting.PlayerCurrentlyVoting == null) ? "NULL" : playerVoting.PlayerCurrentlyVoting.Name) + "|");
			playerTarget = vote.PlayerVoted;
			Wagon currentWagon = null;
			Wagon newWagon = null;


			//Player is already voting there.
			if ((playerVoting.PlayerCurrentlyVoting != null) && ((playerTarget != null) && (playerVoting.PlayerCurrentlyVoting.Name.Equals(playerTarget.Name)))) {
				//Debug.Log("ALREADY VOTING THERE.");


				return;
			} else {
				//Debug.Log("HI");
				//Unvote required
				if ((playerTarget == null) || (playerTarget.Name.ContainsIgnoreCase ("Unvote"))) {
					
					currentWagon = getWagonByTarget (playerVoting.PlayerCurrentlyVoting);
					if (currentWagon != null) {
						//currentWagon.removeVote (playerVoting);
						currentWagon.removeVote(vote);
						//addPlayerNotVoting (playerVoting);
						playerVoting.PlayerCurrentlyVoting = null;
						return;
					}
					playerVoting.PlayerCurrentlyVoting = null;
					return;
				}

				//This is a swap of vote.
				if (playerVoting.PlayerCurrentlyVoting != null) {
					//Debug.Log ("PLAYER VOTING NAME:|" + playerVoting.Name + "| VOTE PLAYER VOTING NAME:|" + vote.PlayerVoting.Name + "|");
					//Debug.Log("SWAP OF VOTE: ");
					currentWagon = getWagonByTarget (playerVoting.PlayerCurrentlyVoting);
					if (currentWagon != null) {
						//currentWagon.removeVote (playerVoting);
						currentWagon.removeVote(vote);
						//addPlayerNotVoting (playerVoting);
					}
				}

				//Debug.Log("ADD THAT VOTE");
				//Add that vote
				newWagon = getWagonByTarget (playerTarget);
				if (newWagon == null) {
					if (playerTarget != null &&  playerTarget.IsAlive)
					{
						wagons.Add (new Wagon (playerTarget, playerVoting, vote.PostNumber,vote.PostBBCode, this.maxThreshold, vote.Timestamp,vote,lSort,sortBy));
					}
					else if (playerTarget == null) {
						
						System.Console.WriteLine ("Wagon creation error. Check your hammer votes for vote:[color][u][b] is the proper order. " + vote.PostNumber);
						throw new Exception("PLAYER TARGET WAS NULL!!!!!!! " + vote.buildDebugOutput());
					}
				} else if (playerTarget.IsAlive) {
					//newWagon.addVote (playerVoting);
					newWagon.addVote(vote);
					
					//removePlayerNotVoting (playerVoting);
					isHammered = newWagon.IsHammered;
					if (isHammered && playerTarget != Wagon.NO_LYNCH_PLAYER) {
						
						days[vote.DayNumber-1].killLynchedPlayer(playerTarget);
						if (playerTarget != null) {

						
							foreach(Player aPlayer in playersInVoteCount)
							{
								if (playerTarget.Name.Equals (aPlayer.Name)) {

									aPlayer.IsDead = true;
									playerTarget.IsDead = true;
								}
							}
						}
						System.Console.WriteLine("Player " + playerTarget.Name + " was hammered in post " + vote.PostNumber + ".");

					}
				}

			}
		}
		catch(NullReferenceException e) {
            System.Console.WriteLine(vote.buildDebugOutput() + "Player Target:|" + playerTarget.Name + "|");
            System.Console.WriteLine(e.ToString ());
            System.Console.WriteLine("A player was missing from post " + vote.PostNumber + ". Please check your replacements.  Debug String: " + vote.buildDebugOutput());
		}
		catch(Exception e) {
            System.Console.WriteLine(e.ToString ());
            System.Console.WriteLine("An unknown error occurred in post number" + vote.PostNumber + ". Please check formatting of file.");
		}
		
	}

	

	private Wagon getWagonByTarget(Player playerTarget)
	{
		if (playerTarget == null) {
			return null;
		}
		foreach (Wagon wagon in wagons) {
			if (wagon.PlayerBeingVoted.Name.Equals( playerTarget.Name)) {
				return wagon;
			}
		}
		return null;
	}

	public void buildLatest(int dayNumber, int priorVCNumber, string flavorBBCode, string deadlineCode, int playersKilledOvernight)
	{

		string displayString = "[area][b][u][size=150][color=#00BFFF]Votecount " + dayNumber + "." + (priorVCNumber + 1) + "[/color][/size][/u][/b]" + History.NEW_LINE_HERE;
		wagons.Sort ((a, b) => -1 * a.CompareTo (b));
		bool firstOne = true;
		bool AWagonWasHammered = false;
		foreach (Wagon wagon in wagons) {
			if (wagon.PlayersVoting.Count > 0)
			{
				if (firstOne == true) {
					displayString = displayString + "[i]";
				}
				displayString = displayString + "[b]" + wagon.PlayerBeingVoted.Name + "(" + wagon.PlayersVoting.Count + ")[/b] ~ " + wagon.playersVotingCommaDelimitedList();
				if (firstOne == true) {
					displayString = displayString + "[/i]" + ((wagon.IsHammered ? " -- HAMMER " : "")) + History.NEW_LINE_HERE;
				} else {
					displayString = displayString + History.NEW_LINE_HERE;
				}
				firstOne = false;
			}
			if (!AWagonWasHammered) {
				AWagonWasHammered = wagon.IsHammered;
			}
		}

		int alivePlayersAtVoteCountStart = playersAlive () + ((AWagonWasHammered == true) ? 1 : 0);


		displayString = displayString + History.NEW_LINE_HERE;
		displayString = displayString + History.NEW_LINE_HERE;
		string playersNotVotingString = getPlayersNotVotingString ();
		string countNotVotingString = playersNotVotingString.Length > 0 ? playersNotVotingString.Split (',').Length + "" : "0";
		displayString = displayString + "Not Voting (" + countNotVotingString + "): " + playersNotVotingString;
		displayString = displayString + History.NEW_LINE_HERE;
		displayString = displayString + History.NEW_LINE_HERE;
		displayString = displayString + "With " + alivePlayersAtVoteCountStart + " alive it takes " + maxThreshold + " to lynch." + History.NEW_LINE_HERE;
       
        
        displayString = displayString + History.NEW_LINE_HERE;
		displayString = displayString + "Day " + dayNumber + " deadline is in " + deadlineCode + "[/area]" + History.NEW_LINE_HERE + History.NEW_LINE_HERE;
		displayString = displayString + "[area=FLAVOR]" + flavorBBCode + "[/area]";       

        latestVCOutput = displayString.Replace (History.NEW_LINE_HERE, System.Environment.NewLine + ((isRestCall) ? "<br/>" : ""));



	}

	private int playersAlive()
	{
		int playersAlive = 0;
		foreach (Player player in playersInVoteCount) {
			if ((player.IsAlive == true) && (!player.IsDead)) {
				playersAlive++;
			}
		}

		return playersAlive;
	}
}
