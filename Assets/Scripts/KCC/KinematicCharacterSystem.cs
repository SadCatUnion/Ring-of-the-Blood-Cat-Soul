using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    public class KinematicCharacterSystem : Singleton<KinematicCharacterSystem>
    {
        public List<KinematicCharacterMotor> characterMotors = new List<KinematicCharacterMotor>();
        public void RegisterCharactorMotor(KinematicCharacterMotor characterMotor)
        {
            characterMotors.Add(characterMotor);
        }
        public void UnregisterCharactorMotor(KinematicCharacterMotor characterMotor)
        {
            characterMotors.Remove(characterMotor);
        }
    }
}