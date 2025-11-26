using System;
using UnityEngine;

namespace MyTestTask.Ranks.Model
{
    [Serializable]
    public class Rank
    {
        [SerializeField] private string id;
        [SerializeField] private int experienceLevel;
        private int _extToNextLevel;
        public string Id => id;
        private int _sequenceIndex;
        public int SequenceIndex => _sequenceIndex;
        public void SetSequenceIndex(int index) => _sequenceIndex = index;
        public int ExtToNextLevel => _extToNextLevel;
        public void SetExtToNextLevel(int ext) => _extToNextLevel = ext;
        public int ExperienceLevel => experienceLevel;
    }
}