using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingGalleryRoundEndMessage : ShootingGalleryMessage
{
    public Winner winner;
}

public class Winner
{
    /// <summary>
    /// The id of the winning entity or if it's a tie a message
    /// </summary>
    public string id;
    /// <summary>
    /// The highest score
    /// </summary>
    public int score;
    /// <summary>
    /// Is there more than one entity with highest score?
    /// </summary>
    public bool tie = false;
    /// <summary>
    /// Array of entity Ids of all that tied for highest score
    /// </summary>
    public string[] tied;
}