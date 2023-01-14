Integers are unsigned and little-endian. Strings use UTF-8 encoding. All sections are directly adjacent with no padding between them.

### Header
offset|size|value
------|----|-----
0     |6   |"FISOBS"
6     |2   |version (max 0)
8     |32  |SHA-256 hash of rest of file
40    |var |sequence of save slots

### Save slot
offset|size|value
------|----|-----
0     |2   |size of slot name, n
2     |n   |slot name string
n+2   |2   |number of Unlock entries
n+4   |var |sequence of Unlock entries

## Unlock entry
offset|size|value
------|----|-----
0     |2   |size of unlock ID, n
2     |n   |unlock ID string
