INCLUDE Cat.ink
VAR ifAcceptedQuest = false
VAR hasVeggies = false // This is the variable to track if veggies have been collected
VAR questCompleted = false

===DinoFirstText===

{questCompleted:
    ->Completed
- else:
    {ifAcceptedQuest: 
        ->Accepted
    - else:
        ->Dino
    }
}
->DONE

===Completed===

Thank you so much for your help! #speaker:Dino #portrait:dino_happy
I'll defintitenlt be at that party!
->DONE

===Accepted===
oh have you got my veggies? #speaker:Dino #portrait:dino_happy

+{hasVeggies}[Yup, here you go!]#complete_quest
    ~hasVeggies = false // Veggies are given, so set the variable to false
    ~questCompleted = true
    ~ifAcceptedQuest = false
    Thank you so much!
    ->DONE
   
+[No, still working it.]
    ok ill be here.
    
->DONE

->DONE 

===Dino===
Welcome to my shop! #speaker:Dino #portrait:dino_neutral


I haven't see you before, you must be a new face!


+[Yup, just moved in!]
    I had a feeling! #portrait:dino_happy
+[Is it that obvious?]
    Yup, never seen anyone one like you around here. #portrait:dino_neutral
    
-But it's always nice to meet new people.

+[House party?] Come to your house warming party?
+[Party at my place!] Oh, a party!?

-Oh I would love to, but sadly my shipment of veggies is late and i can't leave my post until i recive them.... #portrait:dino_sad

+[I can help!] #start_quest
    You would bring them for me? Fantastic! They should be at the post office! #portrait:dino_happy
     ~ifAcceptedQuest = true
    ->DONE

+[Sucks to be you.]
    ... #portrait:dino_sad
    ->DONE
    

    
