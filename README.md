# TextTypeParser
parser to gather data on how text was typed. mainly to generate keyboard layouts

# sample tables
output tables link, mind the dot:
https://docs(dot)google.com/spreadsheets/d/1dH-8kx71XQdJL5QQ836vyw8oO08f4RKWHb2RnNKTmiw



# Algorithm

## Data gathering

- get counts of each key
- get adjacencies (seems like only nearst make any sense, and directionality can be ignored.)

## Data weighting

W of adjacency = sum of char count / incidents.
bigger W means more times keys are near each other.



placement adjacency W = sum ow weigts/ count of characters.

## Layout generation

1. set priority groups based on where fingers have to spend most of the time.
2. define templates for scanning surroundings, like "maximize wights to left and right, and minimize keys above and below"
3. for each priority group take same amount of characters with highest count. find Place and Char with maximum required weight. (for example we want only to minimize weight, so gathered metric must be negative)

