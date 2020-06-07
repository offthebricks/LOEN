# LOEN
Lean Object Encoding Notation

Introducing **Lean Object Encoding Notation** (LOEN, pronounced "loan"), a form of encoding notation based on JSON. LOEN takes the next steps in the path set out by JSON, and focuses on removing anything that isn't needed.

Read a comparison of LOEN vs JSON here: https://www.offthebricks.com/lean-object-encoding-alternative-to-json

LOEN is versatile in its syntax, and its parser is even fully compatible with JSON. LOEN takes the wordiness of JSON, and trims it down to just what you really need. Additionally LOEN handles repeating objects better by stripping out the duplicate property names. This saves space, and the properties are fully restored when parsed. Here's and example.

JSON array:
```
[
   {"id": 1, "abbreviation": "appl", "name": "Apple"},
   {"id": 2, "abbreviation": "pear", "name": "Pear"},
   {"id": 3, "abbreviation": "bana", "name": "Banana"},
   {"id": 4, "abbreviation": "bkby", "name": "Blackberry"},
   {"id": 5, "abbreviation": "strw", "name": "Stawberry"},
   {"id": 5, "abbreviation": "pech", "name": "Peach"},
   {"id": 6, "abbreviation": "plum", "name": "Plum"}
]
```

LOEN compressed array:
```
<
   [:id :abbreviation :name]
   [+1 :appl :Apple]
   [+2 :pear :Pear]
   [+3 :bana :Banana]
   [+4 :bkby :Blackberry]
   [+5 :strw :Stawberry]
   [+5 :pech :Peach]
   [+6 :plum :Plum]
>
```

The LOEN project is not intended for building libraries, but rather to develop the encoding into a standard from which libraries may be developed. Some libraries will be developed and maintained as examples, to demonstrate LOEN and encourage development; I highly encourage anyone with resources, skills, and/or interest, to develop new and/or improved LOEN libraries. With any luck, we'll one day be able to rid ourselves of all those annoying extra double quotes and commas!

Check out the `libraries` folder for some example libraries to try out LOEN.
Check out the `specifications` folder for details on specifics of the encoding system.
