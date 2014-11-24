using SmartStore.Core;
using SmartStore.Core.Domain.Polls;

namespace SmartStore.Services.Polls
{
    /// <summary>
    /// Poll service interface
    /// </summary>
    public partial interface IPollService
    {
        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="pollId">The poll identifier</param>
        /// <returns>Poll</returns>
        Poll GetPollById(int pollId);

        /// <summary>
        /// Gets a poll
        /// </summary>
        /// <param name="systemKeyword">The poll system keyword</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Poll</returns>
		Poll GetPollBySystemKeyword(string systemKeyword, int languageId, bool showHidden = false);
        
        /// <summary>
        /// Gets poll collection
        /// </summary>
        /// <param name="languageId">Language identifier. 0 if you want to get all polls</param>
        /// <param name="loadShownOnHomePageOnly">Retrieve only shown on home page polls</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Poll collection</returns>
        IPagedList<Poll> GetPolls(int languageId, bool loadShownOnHomePageOnly, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Deletes a poll
        /// </summary>
        /// <param name="poll">The poll</param>
        void DeletePoll(Poll poll);

        /// <summary>
        /// Inserts a poll
        /// </summary>
        /// <param name="poll">Poll</param>
        void InsertPoll(Poll poll);

        /// <summary>
        /// Updates the poll
        /// </summary>
        /// <param name="poll">Poll</param>
        void UpdatePoll(Poll poll);
        
        /// <summary>
        /// Gets a poll answer
        /// </summary>
        /// <param name="pollAnswerId">Poll answer identifier</param>
        /// <returns>Poll answer</returns>
        PollAnswer GetPollAnswerById(int pollAnswerId);
        
        /// <summary>
        /// Deletes a poll answer
        /// </summary>
        /// <param name="pollAnswer">Poll answer</param>
        void DeletePollAnswer(PollAnswer pollAnswer);

        /// <summary>
        /// Gets a value indicating whether customer already vited for this poll
        /// </summary>
        /// <param name="pollId">Poll identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>Result</returns>
        bool AlreadyVoted(int pollId, int customerId);
    }
}
