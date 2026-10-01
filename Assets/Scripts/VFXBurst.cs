using UnityEngine;

public static class VFXBurst
{
    public static void Spawn(Vector3 position, Color color, int count = 14)
    {
        GameObject go = new GameObject("VFX Burst");
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 4.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.12f);
        main.startColor = color;
        main.gravityModifier = 0.25f;
        main.maxParticles = 64;

        var emission = ps.emission;
        emission.enabled = false;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f;

        ps.Emit(count);
        Object.Destroy(go, 1.5f);
    }
}
