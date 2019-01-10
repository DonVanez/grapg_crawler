import numpy as np

SampleSize = 15#00
Companies = 10#00
Persons = 10#00

sample = []
while len(sample) < SampleSize:
#for i in range(10):
	#local_company = np.random.randint(1, Companies)
	#local_person = np.random.randint(1, Persons)
	local_pair = (np.random.randint(1, Companies), np.random.randint(1, Persons))
	if local_pair not in sample:
		sample.append(local_pair)

with open('generated.csv', 'w') as output:
	for i in sample:
		output.write('{};{}\n'.format(i[0], i[1]))