Harmonic Hero - README
Game Overview

Harmonic Hero is a rhythm game where players must hit notes corresponding to tracks in sync with the music. Players are challenged to maintain combo streaks by pressing the correct keys when the notes align with the hit bar. The game features customizable song selection and difficulty levels, offering a dynamic rhythm experience.
Controls

    A: Track 1
    S: Track 2
    D: Track 3
    F: Track 4
    Space: Track 5

Gameplay

    Press the corresponding key for each track (A, S, D, F, Space) when the note aligns with the hit bar at the center.
    A note is considered missed if it passes the hit bar without being pressed.
    Combos continue until a note is missed.
    Combo streaks are an important part of achieving high scores.

Loading a Song

    Navigate to the Song List Menu to load a song.
    Select a song from the list.
    Choose a MIDI channel to adjust the instrument tracks (typically lead instruments are suggested).
    Select a difficulty to tailor the challenge. The higher the difficulty, the more complex the note patterns.

Note: Results and gameplay will vary depending on the song and selected difficulty.
Tips

    Focus on the lead instrument tracks for an optimal experience.
    Keep an eye on the hit bar to maintain combos and avoid missed notes.
    Try different difficulty levels to refine your skills.

Enjoy the rhythm and aim for the highest score possible!


Rubric
Readme- In Project
Game Mechanic- Simple Rhythm Game
Menu Screen- Multiple Canvas Menus
Pause- Top right of play field
Background Music- Plays in menus
Sound Effects- Button click, results screen (in-game was terrible)
Particle Effects- On note hit in game
Coroutine- PlayWithDelay in SongManager and I sitched an update to a co-routine for fade in View
Visible Scoreboard- On play screen and results screen
Game Timer- Bar on top of play screen
Game Elements- ...well it isn't one of those games but you collect notes I guess

Comments on design doc:
Player control: As described
Basic Gameplay: As described
Sound and Effects: Sound as described, was not able to get to optional background elements timed to the music.  It would be very easy though with the MIDI events, as timing events to the music is the entire scope of the game
Gameplay Mechanics: As described, but was not able to get to optional power-ups in the game.  Would have been very easy with the combo counter and a key to use powerups
User Interface: As described
Other Features: MIDI keyboard support would most likely not be hard with a plugin that I cant remember the name of.  That would have only been useful with the isomorphic keyboard playfield.  That is the eventual fate of this game, and I have learned enough to create tool for learning the keyboard.
The project timeline was not adhered to.  Getting setup was easy, but #2, loading MIDI files and dropping notes, took about half of the project time.  That make it hard to move forward because all the features depended on the game engine working
I did do one part of the backlog.  I did map the notes to a single octave pentatonic scale.  Pretty necessary to fell like you are keeping time in the song.
The final project looked nothing like the project sketch, and I am glad that I did not try to change the layout of playfield.  It was more than enough integrating the MIDI API into the game engine.