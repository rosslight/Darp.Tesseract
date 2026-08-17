using Shouldly;
using Xunit;

namespace Darp.Tesseract.Native.IntegrationTests;

public sealed class KinematicsTests
{
    [Fact]
    public void AbbIrb2400InverseKinematicsReturnsJointSolutions()
    {
        using var parameters = CreateAbbIrb2400Parameters();
        using var jointNames = new StringVector(new[]
        {
            "joint_1", "joint_2", "joint_3", "joint_4", "joint_5", "joint_6",
        });
        using var solver = new OPWInvKin(parameters, "base_link", "tool0", jointNames);
        using var target = new Isometry3d();
        target.setTranslation(1, 0, 1.306);
        target.setQuaternion(0, 0, 0, 1);
        using var targets = new TransformMap();
        targets.set("tool0", target);
        using var seed = new VectorXd(6);

        using var solutions = solver.calcInvKin(targets, seed);

        solutions.size().ShouldBeGreaterThan(0);
        solver.getSolverName().ShouldBe("OPWInvKin");
        solver.numJoints().ShouldBe(6);
        for (var solutionIndex = 0; solutionIndex < solutions.size(); solutionIndex++)
        {
            using var solution = solutions.get(solutionIndex);
            solution.size().ShouldBe(6);
            for (var jointIndex = 0; jointIndex < solution.size(); jointIndex++)
                double.IsFinite(solution.get(jointIndex)).ShouldBeTrue();
        }
    }

    [Fact]
    public void GeneratedEigenAndParameterBindingsValidateIndices()
    {
        using var parameters = new OPWParameters();
        using var vector = new VectorXd(6);

        Should.Throw<IndexOutOfRangeException>(() => parameters.setOffset(6, 0));
        Should.Throw<ArgumentException>(() => parameters.setSignCorrection(0, 0));
        Should.Throw<IndexOutOfRangeException>(() => vector.get(6));
    }

    private static OPWParameters CreateAbbIrb2400Parameters()
    {
        var parameters = new OPWParameters
        {
            a1 = 0.100,
            a2 = -0.135,
            b = 0,
            c1 = 0.615,
            c2 = 0.705,
            c3 = 0.755,
            c4 = 0.085,
        };
        parameters.setOffset(2, -Math.PI / 2);
        return parameters;
    }
}
