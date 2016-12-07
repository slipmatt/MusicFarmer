﻿using UnitOfWork;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using MediaFarmer.Context.Repositories;
using MediaFarmer.ViewModels;
using MusicFarmer.Data;
using WMPLib;
using MediaFarmer.PlayerService.Classes;

namespace MediaFarmer.PlayerService
{
    public sealed class ThreadedTimers
    {
        private static readonly Lazy<ThreadedTimers> _lazy = new Lazy<ThreadedTimers>(() => new ThreadedTimers());
        public static ThreadedTimers Instance { get { return _lazy.Value; } }

        public System.Threading.Timer RefreshTrackQueue { get; set; }
        public bool IsShuttingDown { get; set; }
        private ThreadedTimers()
        {
            IsShuttingDown = false;
        }
    }

    public class ThreadedTimerExecutions
    {
        private static List<SettingValueViewModel> _settings { get; set; }
        private static MediaPlayerController _player { get; set; }
        private static PlayListController _playList { get; set; }

        public static void RefreshTrackQueue(object obj)
        {
            var track = new PlayHistoryViewModel();
            _playList = PlayListController.Instance;
            _player = MediaPlayerController.Instance;
            using (var uow = new Uow(new MusicFarmerEntities()))
            {
                var repoSettings = new RepositorySettings(uow);
                var repoVotes = new RepositoryVote(uow);

                _playList.InitializePlaylist(uow);
                _player.InitializePlayer();
                _playList.RefreshPlaylist();

                _settings = repoSettings.GetAllSettings();
                SetPlayerSettings();

                if (_player.IsMuted)
                {
                    return;
                }

                if (_player.IsPlaying() || (_player.GetDuration()>_player.GetElapsed()))
                {
                    SetPlayerVolumeBasedOnVotes(repoVotes.GetUpVotes(track.PlayHistoryId).Count, repoVotes.GetDownVotes(track.PlayHistoryId).Count);
                }
                else
                {
                    ChangeTrack();
                }

                if (_playList.IsPlayingTrack() && !(_player.IsPlaying()))
                {
                    var ph = _playList.GetPlayingTrack();
                    track = _playList.GetPlayingTrack();
                    PlayTrack(track.Track.TrackURL);
                }

                else if (_playList.HasTrackQueued() && !(_player.IsPlaying()))
                {
                    track = _playList.GetNextQueuedTrack();
                    PlayTrack(track.Track.TrackURL);
                }
                else
                {
                    //Jukebox
                }

                

            }
        }

        private static void ChangeTrack()
        {
            if (_player.PlayedTrack)
            {
                _playList.StopCurrentTrack();
            }
            _playList.GetNextQueuedTrack();
        }

        private static void SetPlayerVolumeBasedOnVotes(int upVote, int downVote)
        {
            var initVolume = _settings.Find(s => s.SettingId == (int)MediaFarmer.Enumerators.Settings.StartVolume).SettingValue;
            var increment = _settings.Find(s => s.SettingId == (int)MediaFarmer.Enumerators.Settings.VolumeIncrements).SettingValue;
            var currentVotes = upVote - downVote;
            _player.SetVolume(initVolume + (increment * currentVotes));
        }

        private static void SetPlayerSettings()
        {
            if (_settings.Any(s => s.Active == true && s.SettingId == (int)MediaFarmer.Enumerators.Settings.MinutesOfSilence))
            {
                _player.Mute();
            }
            else
            {
                _player.UnMute();
                _player.SetVolume(_settings.Find(s => s.SettingId == (int)MediaFarmer.Enumerators.Settings.StartVolume).SettingValue);
            }
        }

        private static void PlayTrack(string Url)
        {
            _player.PlayTrack(Url);
        }
    }
}