import pandas as pd
import matplotlib.pyplot as plt

energyData_directory = "/Users/anirudhmuthukumar/Downloads/590G/Unity Projects/HW3/Assignment3a_starter/EnergyData/"
plot_directory = "/Users/anirudhmuthukumar/Downloads/590G/Unity Projects/HW3/Assignment3a_starter/Plots/"

time_steps = [0.001, 0.01, 0.05, 0.1, 0.2, 0.3]
v = 0
p = 0
odes = ["euler", "improved-euler", "rk", "semi-implicit"]
ct = 0

for ode in odes:
    errorRate = {}
    
    for time_step in time_steps:

        fileName = str(time_step) + '_v_'+ str(v) + "_p_" + str(p) + "_" + ode + ".csv"
        energyFile = energyData_directory + fileName

        energy = pd.read_csv(energyFile)
        print(energy.columns)

        print(ct+1, fileName)

        print(float(energy["Total Energy"][0]), float(energy["Total Energy"][len(energy)-1]))

        # compute error for each timestep 
        errorRate[time_step] = abs(float(energy["Total Energy"][0]) - float(energy["Total Energy"][len(energy)-1]))
    
    plt.figure()
    plt.plot(time_steps, [error for time_step, error in errorRate.items()])
    plt.xlabel("Time step")
    plt.ylabel("Error")
    plt.title("Error plot for " + ode)
    plt.show()

