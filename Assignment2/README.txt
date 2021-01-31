Brodey Yendall - s3718834
Dylan Sbogar - s3718036

Trello Board: https://trello.com/b/Hio0xWER/assignment-2

---- Use of Records ----
Since the use of records in C# is mostly a performance-based feature, we took it upon ourselves to find any area in which large volumes of data would need to be stored amongst our many models in this assessment, but
also attmepted to determine which of our existing models we could turn into these immutable Records. The easiest solution to this we determined was the Transactions model. Since when we create a new transaction
object, its contents are not to be modified afterwards. This gave us the opportunity to make this an immutable record, and taking into consideration that Transactions are the most common element in the database, this
works to help performance as we now only need to pass around immutable references, as opposed to value-types that we don't change anyway; thus saving having to copy the same data every time you move around the
program which becomes more beneficial at scale.

---- GitHub Repository ----
Over the past thirteen days of this assessment, we have made roughly 140 commits (give or take a few) in the repository over numerous branches. We each tried to make at least a singular commit on a daily basis to our respective branches,
so that in the event that something went wrong we would be able to revert to the moment when everything was working to replicate our issues. We worked with a proper Git Flow for this assessment too, with our develop branch being used to merge 
major features in from our child branches, usually being indicated by a functional name such as feature/implement-atm-transaction. Each of us would work on a single branch - and thus a single feature at a time. This made it so we would run
into as little merge conflicts as possible throughout the development process. Once we felt the major feature had been complete on our branch, the creator would send out a pull request for the other to review; and provide feedback if it
were necessary - before approving the request to be merged into the develop branch. Once we felt the assessment was complete, we merged the develop branch into the master branch; indicating its completion as one would usually save the master 
branch for 'final' releases.