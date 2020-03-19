import pandas as pd
import matplotlib.pyplot as plt

energyData_directory = "/Users/anirudhmuthukumar/Downloads/590G/Unity Projects/HW3/Assignment3a_starter/EnergyData/"
plot_directory = "/Users/anirudhmuthukumar/Downloads/590G/Unity Projects/HW3/Assignment3a_starter/Plots/"

Vs = [0, 2]
Ps = [0, 0.5]
odes = ["euler", "improved-euler", "rk", "semi-implicit"]
ct = 0
print()

for time_step in [0.05]:
    for v in Vs:
        for p in Ps:
            if v==2 and p==0.5:
                continue
            else:
                for ode in odes:
                    
                    fileName = str(time_step) + '_v_'+ str(v) + "_p_" + str(p) + "_" + ode + ".csv"
                    energyFile = energyData_directory + fileName

                    print(ct+1, fileName)

                    energy = pd.read_csv(energyFile)

                    # plt.figure()
                    plotTitle = "Time Step = %ss, Angular Velocity = %s, Friction Coefficient = %s, ODE = %s" %(time_step, v, p, ode)
                    energy.plot(x = "Time", y = ["Kinetic Energy", "Potential Energy", "Total Energy"], title = plotTitle, figsize = (10, 10), kind = "line").legend(bbox_to_anchor=(0.75, -0.06), ncol = 3)
                    plt.ylabel("Energy")
                    plt.savefig(plot_directory + fileName[:-3] + "png")

                    ct += 1

print()