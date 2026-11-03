using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GslvMission
{
    public enum MissionCommand
    {
        Scene,          // label only, marks where a scene starts
        Gate,           // pause the Timeline until the learner clicks the button with this id (or answers the quiz)
        Quiz,           // show quiz question number <argument> (1-5) on the quiz panel
        ShowSpace,      // swap the launch pad for the space environment
        ShowLaunchpad,  // swap back
        EndMenu         // show the end menu (RESTART / EXIT); it stays up after the Timeline has finished
    }

    /// <summary>
    /// Timeline marker read by MissionFlow. Put it on the Timeline's marker track; the command and
    /// argument say what should happen when playback reaches it.
    /// </summary>
    [Serializable]
    [System.ComponentModel.DisplayName("Mission Marker")]
    public class MissionMarker : Marker, INotification
    {
        public MissionCommand command;
        public string argument;

        public PropertyName id { get { return new PropertyName("MissionMarker"); } }
    }
}
