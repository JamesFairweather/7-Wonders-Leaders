namespace SevenWonders
{
    // I have a feeling that we really only need 3 phases: LeaderDraft (drawing 4
    // leaders before the game), Leader Recruitment, and Playing.  Playing needs
    // I think just two substates: Regular turn or Extra turn.  Extra turn would
    // be any turn where the game has to wait for one player to choose a
    // card/action (i.e. Babylon B, Halikarnassos, Solomon, Roma B, China)

    public enum GamePhase
    {
        None,
        // WaitingForPlayers,   // not used presently
        // Start,               // not used presently
        LeaderDraft,            // drafting leaders (i.e. before Age 1)
        LeaderRecruitment,      // playing a leader card at the start of each Age
        Playing,                // normal turn
        Babylon,                // Waiting for Babylon to play its last card in the age.
        Halikarnassos,          // Waiting for Halikarnassos to play from the discard pile.
        RomaB,                  // Waiting  for Roma (B) to play a leader after building 2nd or 3rd wonder stage
        Solomon,                // Waiting for Solomon to play from the discard pile (can coincide with Halikarnassos if Rome B builds its 2nd or 3rd wonder stage and the player plays Solomon)
        Courtesan,              // Waiting for a player to select a leader to copy
        End,
    };
}
