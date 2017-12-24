using System.Collections;
using System.Collections.Generic;
using System;

public class History  {

	List<Wagon> historicalWagons;
	public const string NEW_LINE_HERE = "NEW_LINE_HERE";
	bool lSort;
    private bool isRestCall;

	public History(bool _lSort, bool _isRestCall)
	{
		historicalWagons = new List<Wagon> ();
		lSort = _lSort;
        isRestCall = _isRestCall;
	}

	public void update(VoteCount vc)
	{
		Wagon clonedWagon = null;

		foreach (Wagon wagon in vc.Wagons) {

			if (wagon.PlayerBeingVoted != null) {
				clonedWagon = ExtensionMethods.DeepClone (wagon);

				if (isDuplicateWagon (clonedWagon) == null) {
					historicalWagons.Add (clonedWagon);
				}
			}

		}
			
	}

	public void debugDumpAllWagons()
	{
		foreach (Wagon wagon in historicalWagons) {
			if (wagon.PlayersVoting.Count > 0) {
				System.Console.WriteLine (wagon.ToString ());
			}
		}

	}

	public string buildDisplayString(int dayNumber, bool simple)
	{
		string returnString = "[spoiler=Day " + dayNumber + "]";
		if (lSort) {
			
			historicalWagons.Sort ();
			int currentLoopLevel = 0;
			foreach (Wagon historicalWagon in historicalWagons) {
				if (historicalWagon.PlayersVoting.Count > 0) {
					if (currentLoopLevel != historicalWagon.L_Level) {
						if (currentLoopLevel != 0) {
							returnString = returnString + "[/area]";
						}
						/*if (historicalWagon.L_Level > 0) {
							returnString = returnString + "[b]L-" + historicalWagon.L_Level + "[/b]" + NEW_LINE_HERE + "[area]";
						} else {
							returnString = returnString + "[b]HAMMER TIME![/b]" + NEW_LINE_HERE + "[area]";
						}*/
						returnString = returnString + NEW_LINE_HERE + "[area]";
						currentLoopLevel = historicalWagon.L_Level;
					}
				
					returnString = returnString + historicalWagon.ToHistoricalDisplayString (!simple) + NEW_LINE_HERE;
				}
			}
			returnString = returnString.Replace (NEW_LINE_HERE, System.Environment.NewLine + ((isRestCall) ? "<br/ >" : "")) + "[/area][/spoiler]";

			return returnString;
		} else {
			returnString = returnString + "[area]";
			historicalWagons.Sort ();
			foreach (Wagon historicalWagon in historicalWagons) {
				if (historicalWagon.PlayersVoting.Count > 0) {
					
					returnString = returnString + historicalWagon.ToHistoricalDisplayString (!simple) + NEW_LINE_HERE;
				}
			}
			returnString = returnString.Replace (NEW_LINE_HERE, System.Environment.NewLine  + ((isRestCall) ? "<br/>" : "")) + "[/area][/spoiler]";
			return returnString;
		}

	}

	public void sortWagons()
	{
		historicalWagons.Sort ();
	}


	public Wagon isDuplicateWagon(Wagon wagon)
	{
		try {
		bool duplicateWagon = false;

		if (historicalWagons.Count == 0) {
			
			return null;
		} else {
			foreach(Wagon historicalWagon in historicalWagons)
			{
				
				if (historicalWagon.PlayerBeingVoted.Name.Equals (wagon.PlayerBeingVoted.Name)) {
					

					if (historicalWagon.L_Level == wagon.L_Level)
					{
						duplicateWagon =  checkDuplicatePlayersVoting (historicalWagon, wagon);

						if (duplicateWagon == true) {
							return historicalWagon;
						}
					}
				}
			}
		}
		return null;
		}
		catch (Exception e){
			System.Console.WriteLine("EXCEPTION: " + e.ToString());
			return null;
		}
	}

	public bool checkDuplicatePlayersVoting(Wagon historicalWagon, Wagon wagon)
	{
		//Need count check in case on different days.
		if (historicalWagon.PlayersVoting.Count != wagon.PlayersVoting.Count) {
			return false;
		}
		bool playerFound = false;
		foreach (Player historicalPlayer in historicalWagon.PlayersVoting) {
			foreach (Player player in wagon.PlayersVoting) {

				if (historicalPlayer.Name.Equals (player.Name)) {
					playerFound = true;
					break;
				}

			}

			if (playerFound == true) {
				playerFound = false;
				continue;
			} else {
				return false;
			}
		}

		return true;
	}
}
