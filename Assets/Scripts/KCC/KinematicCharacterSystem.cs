using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KCC
{
    public class KinematicCharacterSystem : Singleton<KinematicCharacterSystem>
    {
        public List<KinematicCharacterMotor> characterMotors = new List<KinematicCharacterMotor>();
        public List<PhysicsMover> physicsMovers = new List<PhysicsMover>();
        public void RegisterCharactorMotor(KinematicCharacterMotor characterMotor)
        {
            characterMotors.Add(characterMotor);
        }
        public void UnregisterCharactorMotor(KinematicCharacterMotor characterMotor)
        {
            characterMotors.Remove(characterMotor);
        }
        public void RegisterPhysicsMover(PhysicsMover physicsMover)
        {
            physicsMovers.Add(physicsMover);
            physicsMover.rigidbody.interpolation = RigidbodyInterpolation.None;
        }
        public void UnregisterPhysicsMover(PhysicsMover physicsMover)
        {
            physicsMovers.Remove(physicsMover);
        }
    }
}