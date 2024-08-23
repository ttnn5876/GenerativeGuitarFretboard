FeatureScript 2411;
import(path : "onshape/std/common.fs", version : "2411.0");

export const FRET_BOUNDS =  {
    (unitless): [1, 24, 100]
    } as IntegerBoundSpec;
    
export const PERPENDICULAR_FRET_BOUNDS = {
    (unitless): [1, 9, 100]
    } as IntegerBoundSpec;

export const SCALE_LENGTH_BOUNDS =  {
    (inch): [1, 25.5, 100],
    (millimeter): 648,
    (centimeter): 64.8,
    (meter): 0.648,
    } as LengthBoundSpec;

export const BODY_OVERLAP_BOUNDS = {
    (inch): [0.1, 0.23, 1],
    (millimeter): 6,
    (centimeter): 0.6,
    (meter): 0.06,
    } as LengthBoundSpec;
    
export const FRETBOARD_RADIUS_BOUNDS = {
    (inch): [10, 16, 25],
    (millimeter): 400,
    (centimeter): 40,
    (meter): 0.4,
    } as LengthBoundSpec;
    
export const BLIND_SLOT_BOUNDS = {
    (millimeter): [0, 1, 10],
    (inch): 0.04,
    (centimeter): 0.1,
    (meter): 0.001,
    } as LengthBoundSpec;
    
export const STRING_SPACING_BOUNDS = {
    (millimeter): [0, 6, 20],
    (inch): 0.23,
    (centimeter): 0.6,
    (meter): 0.006,
    } as LengthBoundSpec;
    
export const BRIDGE_STRING_SPACING_BOUNDS = {
    (millimeter): [0, 10.6, 20],
    (inch): 0.417,
    (centimeter): 1.06,
    (meter): 0.0106,
    } as LengthBoundSpec;
    
export const EDGE_SPACING_BOUNDS = {
    (millimeter): [0, 3, 20],
    (inch): 0.118,
    (centimeter): 0.3,
    (meter): 0.003,
    } as LengthBoundSpec;
    
export const FRETBOARD_THICKNESS_BOUNDS = {
    (millimeter): [0.001, 6, 20],
    (inch): 0.23,
    (centimeter): 0.6,
    (meter): 0.006,
    } as LengthBoundSpec;

export const STRING_COUNT_BOUNDS = {
    (unitless): [1, 6, 50]
    } as IntegerBoundSpec;

export const FRET_SLOT_DEPTH_BOUNDS = {
    (millimeter): [0.00001, 1.4, 6],
    } as LengthBoundSpec;

export const FRET_SLOT_WIDTH_BOUNDS = {
    (millimeter): [0.00001, 0.5, 6],
    } as LengthBoundSpec;

annotation { "Feature Type Name" : "Generative Fretboard" }
export const generativeFretboard = defineFeature(function(context is Context, id is Id, definition is map)
    precondition {
        annotation { "Name" : "Scale Length"}
        isLength(definition.scaleLength, SCALE_LENGTH_BOUNDS);

        annotation { "Name" : "Fret Count"}
        isInteger(definition.fretCount, FRET_BOUNDS);

        annotation { "Name" : "String Count"}
        isInteger(definition.stringCount, STRING_COUNT_BOUNDS);
        
        annotation { "Name" : "Fretboard Radius"}
        isLength(definition.fretboardRadius, FRETBOARD_RADIUS_BOUNDS);
        
        annotation { "Name" : "Multiscale"}
        definition.multiscale is boolean;
        
        if (definition.multiscale) {
            annotation { "Group Name" : "Multiscale Options", "Collapsed By Default": true, "Driving Parameter" : "multiscale" } {
                annotation { "Name": "Highest String Scale Length" }
                isLength(definition.highMultiscaleLength, SCALE_LENGTH_BOUNDS);
                
                annotation { "Name": "Lowest String Scale Length" }
                isLength(definition.lowMultiscaleLength, SCALE_LENGTH_BOUNDS);

                annotation { "Name": "Perpendicular Fret Number" }
                isInteger(definition.perpendicularFret, PERPENDICULAR_FRET_BOUNDS);
            }
        }
        
        annotation { "Name" : "Spacing Options" }
        definition.spacingOptions is boolean;
        
        if (definition.spacingOptions) {
            annotation { "Group Name" : "Spacing Options", "Collapsed By Default" : true, "Driving Parameter" : "spacingOptions" } {
                annotation { "Name" : "Nut String Spacing"}
                isLength(definition.stringSpacing, STRING_SPACING_BOUNDS);
                
                annotation { "Name" : "Bridge String Spacing"}
                isLength(definition.bridgeStringSpacing, BRIDGE_STRING_SPACING_BOUNDS);
                
                annotation { "Name" : "String Edge Spacing"}
                isLength(definition.edgeSpacing, EDGE_SPACING_BOUNDS);
            }
        }
            
        annotation { "Name" : "Fret Slot Options" }
        definition.slotOptions is boolean;
        
        if (definition.slotOptions) {
            annotation { "Group Name" : "Fret Slot Options", "Collapsed By Default" : true, "Driving Parameter" : "slotOptions" } {
                annotation { "Name" : "Slot Depth"}
                isLength(definition.fretSlotDepth, FRET_SLOT_DEPTH_BOUNDS);
                
                annotation { "Name" : "Slot Width"}
                isLength(definition.fretSlotWidth, FRET_SLOT_WIDTH_BOUNDS);
            }
        }
        
        annotation { "Name" : "Advanced Options"}
        definition.advancedOptions is boolean;
        
        if (definition.advancedOptions) {
            annotation { "Group Name" : "Advanced Options", "Collapsed By Default" : true, "Driving Parameter" : "advancedOptions" } {
                annotation { "Name" : "Blind Fret Slot Thickness"}
                isLength(definition.blindSlotThickness, BLIND_SLOT_BOUNDS);
                
                annotation { "Name" : "Fretboard Thickness"}
                isLength(definition.fretboardThicknes, FRETBOARD_THICKNESS_BOUNDS);
                
                annotation { "Name" : "Body Overlap"}
                isLength(definition.bodyOverlap, BODY_OVERLAP_BOUNDS);
            }
        }
        
    } {
        
        // Validate required inputs
        if (definition.perpendicularFret > definition.fretCount || 
        definition.perpendicularFret < 1 ||
        definition.highMultiscaleLength > definition.lowMultiscaleLength) {
            throw { message : ErrorStringEnum.INVALID_INPUT };
        }
        
        if (definition.multiscale) {
            
            if (definition.stringCount < 2) {
                throw { message : ErrorStringEnum.INVALID_INPUT };
            }
            
            // Calculate fret offsets
            var highFretOffsets = makeArray(definition.fretCount + 1, 0);
            var lowFretOffsets = makeArray(definition.fretCount + 1, 0);
            var highCurrentFret = definition.highMultiscaleLength;
            var lowCurrentFret = definition.lowMultiscaleLength;
            
            for (var fretNum = 1; fretNum <= definition.fretCount; fretNum += 1) {
                highCurrentFret = highCurrentFret / (2 ^ (1/12));
                highFretOffsets[fretNum] = definition.highMultiscaleLength - highCurrentFret;
                
                lowCurrentFret = lowCurrentFret / (2 ^ (1/12));
                lowFretOffsets[fretNum] = definition.lowMultiscaleLength - lowCurrentFret;            
            }
            
            // Calculate fret locations
            var lowFretLocations = makeArray(definition.fretCount + 1, 0);
            var highFretLocations = makeArray(definition.fretCount + 1, 0);
            var middleFretLocations = makeArray(definition.fretCount + 1, 0);
            
            var fretboardTopHorizontalWidth = (definition.stringCount - 1) * definition.stringSpacing;
            var halfFretboardTopHorizontalWidth = fretboardTopHorizontalWidth / 2;
            
            var fretboardBottomHorizontalWidth = (definition.stringCount - 1) * definition.bridgeStringSpacing;
            var halfFretboardBottomHorizontalWidth = fretboardBottomHorizontalWidth / 2;
            
            // Onshape math is wild
            var fretboardVerticalLength = ((definition.lowMultiscaleLength / millimeter) ^ 2 - (halfFretboardBottomHorizontalWidth / millimeter - halfFretboardTopHorizontalWidth / millimeter) ^ 2) ^ 0.5 * millimeter;
                        
            var lowStringAngle = tan((halfFretboardBottomHorizontalWidth - halfFretboardTopHorizontalWidth) / fretboardVerticalLength * radian);
            
            // Find each fret low and high point and create a fret 3d object    
            for (var fretNum = 1; fretNum <= definition.fretCount; fretNum += 1) {
                lowFretLocations[fretNum] = vector(lowFretOffsets[fretNum] * sin(lowStringAngle * radian) + halfFretboardTopHorizontalWidth, lowFretOffsets[fretNum] * cos(lowStringAngle * radian));
            }
                        
            var xReference = -lowFretOffsets[definition.perpendicularFret] * sin(lowStringAngle * radian) - halfFretboardTopHorizontalWidth;
            var yReference = lowFretOffsets[definition.perpendicularFret] * cos(lowStringAngle * radian);
            
            for (var fretNum = 1; fretNum <= definition.fretCount; fretNum +=1) {
                highFretLocations[fretNum] = vector(xReference - (highFretOffsets[definition.perpendicularFret] - highFretOffsets[fretNum]) * sin(-lowStringAngle * radian),
                                                      yReference - (highFretOffsets[definition.perpendicularFret] - highFretOffsets[fretNum]) * cos(-lowStringAngle * radian));
                middleFretLocations[fretNum] = vector(0 * millimeter, (highFretLocations[fretNum][1] + lowFretLocations[fretNum][1]) / 2);                
            }
            
            // Find the outer corners of the fretboard
            // The edge spacing can be horizontal or colinear with the frets. I went with horizontal.
            var highNutDirectionVector = sqrt((highFretLocations[2][0] - highFretLocations[1][0]) ^ 2 + (highFretLocations[2][1] - highFretLocations[1][1]) ^ 2);
            var lowNutDirectionVector = sqrt((lowFretLocations[2][0] - lowFretLocations[1][0]) ^ 2 + (lowFretLocations[2][1] - lowFretLocations[1][1]) ^ 2);
            var highBridgeDistance = definition.highMultiscaleLength - highFretOffsets[1];
            
            var lowNutCorner = vector(halfFretboardTopHorizontalWidth + definition.edgeSpacing, 0 * millimeter);
            var lowBridgeCorner = vector(halfFretboardBottomHorizontalWidth + definition.edgeSpacing, fretboardVerticalLength);
            var highBridgeCorner = vector(highFretLocations[1][0] + highBridgeDistance * (highFretLocations[2][0] - highFretLocations[1][0]) / highNutDirectionVector - definition.edgeSpacing,
                                       highFretLocations[1][1] + highBridgeDistance * (highFretLocations[2][1] - highFretLocations[1][1]) / highNutDirectionVector);
                                       
            var highNutCorner = vector(highFretLocations[1][0] - highFretOffsets[1] * (highFretLocations[2][0] - highFretLocations[1][0]) / highNutDirectionVector - definition.edgeSpacing,
                                       highFretLocations[1][1] - highFretOffsets[1] * (highFretLocations[2][1] - highFretLocations[1][1]) / highNutDirectionVector);
            
            // Create fretboard blank
            var fretboardBlankTop = newSketch(context, id + "fretboardBlankTop", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            
            skLineSegment(fretboardBlankTop, "fretboardBlankNut", {
                    "start" : lowNutCorner,
                    "end" : highNutCorner
            });
            
            skLineSegment(fretboardBlankTop, "fretboardBlankBridge", {
                    "start" : lowBridgeCorner,
                    "end" : highBridgeCorner
            });
            
            skLineSegment(fretboardBlankTop, "fretboardBlankHighOutline", {
                    "start" : highNutCorner,
                    "end" : highBridgeCorner
            });
            
            skLineSegment(fretboardBlankTop, "fretboardBlankLowOutline", {
                    "start" : lowNutCorner,
                    "end" : lowBridgeCorner
            });
            
            skSolve(fretboardBlankTop);
            
            opExtrude(context, id + "fretboardBlank", {
                    "entities" : qSketchRegion(id + "fretboardBlankTop"),
                    "direction" : -evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "fretboardBlankTop", false) }).normal,
                    "endBound" : BoundingType.BLIND,
                    "endDepth" : definition.fretboardThicknes
            });
            
            opDeleteBodies(context, id + "deleteSketchfretboardBlankTop", { "entities" : qCreatedBy(id + "fretboardBlankTop") });
            
            // Create fret slots
            for (var fretNum = 1; fretNum <= definition.fretCount; fretNum +=1) {     
                
                // Draw fret profiles
                var fretProfile = newSketch(context, id + ("fretProfile" ~ fretNum), {
                        "sketchPlane" : qCreatedBy(makeId("Right"), EntityType.FACE)
                });
                
                skRectangle(fretProfile, "fretSlotsProfile" ~ fretNum, {
                        "firstCorner" : vector(middleFretLocations[fretNum][1] + definition.fretSlotWidth / 2, 0 * millimeter),
                        "secondCorner" : vector(middleFretLocations[fretNum][1] - definition.fretSlotWidth / 2, -definition.fretSlotDepth)
                });
                
                skSolve(fretProfile);
                
                // Get fret direction
                var fretDirection = vector(lowFretLocations[fretNum][0] - highFretLocations[fretNum][0], lowFretLocations[fretNum][1] - highFretLocations[fretNum][1], 0 * millimeter);
                                
                // Extrude the fret 3d shape using the profile
                opExtrude(context, id + ("fretSlot" ~ fretNum), {
                        "entities" : qSketchRegion(id + ("fretProfile" ~ fretNum)),
                        "direction" : normalize(fretDirection / millimeter),
                        "endBound" : BoundingType.UP_TO_NEXT,
                        "isStartBoundOpposite" : true,
                        "endTranslationalOffset" : -definition.blindSlotThickness,
                        "startBound" : BoundingType.UP_TO_NEXT,
                        "startTranslationalOffset" : -definition.blindSlotThickness
                });
                
                opDeleteBodies(context, id + ("deleteFretProfile" ~ fretNum), { "entities" : qCreatedBy(id + ("fretProfile" ~ fretNum)) });
            }
                          
            // Remove frets from the fretboard blank
            for (var fretNum = 1; fretNum <= definition.fretCount; fretNum +=1) {
                opBoolean(context, id + ("fretSlotsCut" ~ fretNum), {
                    "tools" : qBodyType(qCreatedBy(id + ("fretSlot" ~ fretNum), EntityType.BODY), BodyType.SOLID),
                    "targets": qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACTION
                });
            }
            
            // Cut excess material after the last fret
            var excessCut = newSketch(context, id + "excessCutTop", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            
            var highLastCutDistance = highFretOffsets[definition.fretCount] + definition.bodyOverlap - highFretOffsets[1];
            var highLastFret = vector(highFretLocations[1][0] + highLastCutDistance * (highFretLocations[2][0] - highFretLocations[1][0]) / highNutDirectionVector - definition.edgeSpacing,
                                      highFretLocations[1][1] + highLastCutDistance * (highFretLocations[2][1] - highFretLocations[1][1]) / highNutDirectionVector);
            
            var lowLastCutDistance = lowFretOffsets[definition.fretCount] + definition.bodyOverlap - lowFretOffsets[1];
            var lowLastFret = vector(lowFretLocations[1][0] + lowLastCutDistance * (lowFretLocations[2][0] - lowFretLocations[1][0]) / lowNutDirectionVector + definition.edgeSpacing,
                                     lowFretLocations[1][1] + lowLastCutDistance * (lowFretLocations[2][1] - lowFretLocations[1][1]) / lowNutDirectionVector);
            
            skLineSegment(excessCut, "excessCutLastFret", {
                    "start" : highLastFret,
                    "end" : lowLastFret
            });
            
            skLineSegment(excessCut, "excessCutHigh", {
                    "start" : highLastFret,
                    "end" : highBridgeCorner
            });
            
            skLineSegment(excessCut, "excessCutLow", {
                    "start" : lowLastFret,
                    "end" : lowBridgeCorner
            });
            
            skLineSegment(excessCut, "excessCutBridge", {
                    "start" : highBridgeCorner,
                    "end" : lowBridgeCorner
            });
            
            skSolve(excessCut);
            
            opExtrude(context, id + "excessCutBody", {
                    "operationType": NewBodyOperationType.REMOVE,
                    "entities" : qSketchRegion(id + "excessCutTop"),
                    "direction" : -evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "excessCutTop", false) }).normal,
                    "endBound" : BoundingType.THROUGH_ALL,
            });
            
            opBoolean(context, id + "excessCut", {
                    "targets" : qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "tools" : qBodyType(qCreatedBy(id + "excessCutBody", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACTION
            });
            
            opDeleteBodies(context, id + "deleteSketchExcessCutTop", { "entities" : qCreatedBy(id + "excessCutTop") });
            
            // Create fretboard radius
            var fretboardRadiusProfile = newSketch(context, id + "fretboardRadiusProfile", {
                "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE),
            });
            
            skCircle(fretboardRadiusProfile, "fretboardRadiusCircle", {
                    "center" : vector(0 * inch, -definition.fretboardRadius),
                    "radius" : definition.fretboardRadius
            });
            
            skSolve(fretboardRadiusProfile);
            
            opExtrude(context, id + "fretboardRadiusBody", {
                    "operationType": NewBodyOperationType.REMOVE,
                    "entities" : qSketchRegion(id + "fretboardRadiusProfile"),
                    "direction" : -evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "fretboardRadiusProfile", false) }).normal,
                    "endBound" : BoundingType.THROUGH_ALL
            });
            
                    
            opBoolean(context, id + "fretboardRadiusCut", {
                    "targets" : qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "tools": qBodyType(qCreatedBy(id + "fretboardRadiusBody", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACT_COMPLEMENT
            });
            
            opDeleteBodies(context, id + "deleteSketchFretboardRadiusProfile", { "entities" : qCreatedBy(id + "fretboardRadiusProfile") });
            
            
        } else {
            
            // Create fretboard blank profiles on a plane at the nut and at the bridge
            var fretboardWidth = ((definition.stringCount - 1) * definition.stringSpacing + definition.edgeSpacing * 2);
            var halfFretboardWidth = fretboardWidth / 2;
            
            var fretboardBlankTop = newSketch(context, id + "fretboardBlankTop", {
                    "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE)
            });
            
            skRectangle(fretboardBlankTop, "fretboardTopProfile", {
                    "firstCorner" : vector(-halfFretboardWidth, 0 * millimeter),
                    "secondCorner" : vector(halfFretboardWidth, -definition.fretboardThicknes)
            });
            
            skSolve(fretboardBlankTop);
                         
            var fretboardBlankBottom = newSketchOnPlane(context, id + "fretboardBlankBottom", {
                    "sketchPlane" : plane(vector(0 * millimeter, definition.scaleLength, 0 * millimeter), vector(0 * millimeter, definition.scaleLength, 0 * millimeter))
            });
            
            var fretboardBottomWidth = (definition.bridgeStringSpacing * (definition.stringCount - 1) + (2 * definition.edgeSpacing)) / 2;
            skRectangle(fretboardBlankBottom, "fretboardBottomProfile", {
                    "firstCorner" : vector(0 * millimeter, -fretboardBottomWidth),
                    "secondCorner" : vector(-definition.fretboardThicknes, fretboardBottomWidth)
            });
            
            skSolve(fretboardBlankBottom);
            
            // Loft the fretboard blank from the profiles
            opLoft(context, id + "fretboardBlank", {
                    "profileSubqueries" : [ qSketchRegion(id + "fretboardBlankTop"), qSketchRegion(id + "fretboardBlankBottom") ],
                    "bodyType" : ToolBodyType.SOLID
            });
            
            opDeleteBodies(context, id + "deleteSketchFretboardBlankTop", { "entities" : qCreatedBy(id + "fretboardBlankTop") });
            opDeleteBodies(context, id + "deleteSketchFretboardBlankBottom", { "entities" : qCreatedBy(id + "fretboardBlankBottom") });
            
            // Calculate fret location
            var fret_locations = makeArray(definition.fretCount + 1, 0);
            var current_fret = definition.scaleLength;
            for (var fret_num = 1; fret_num <= definition.fretCount; fret_num += 1) {
                current_fret = current_fret / (2 ^ (1/12));
                fret_locations[fret_num] = definition.scaleLength - current_fret;            
            }
            
            // Slots are extruded both ways from the Right plane
            var fretSlots = newSketch(context, id + "fretSlotsProfile", {
                "sketchPlane" : qCreatedBy(makeId("Right"), EntityType.FACE)
            });
            
            for (var fret_num = 1; fret_num <= definition.fretCount; fret_num += 1) {
                // Placeholder dimension for fret tangs - 1.4 * 0.5 mm
                skRectangle(fretSlots, "fretSlotsProfile"  ~ fret_num, {
                        "firstCorner" : vector(fret_locations[fret_num] + definition.fretSlotWidth / 2, 0 * millimeter),
                        "secondCorner" : vector(fret_locations[fret_num] - definition.fretSlotWidth / 2, -definition.fretSlotDepth)
                });
            }
    
            skSolve(fretSlots);
    
            // Extrude fret shape both ways
            opExtrude(context, id + "fretSlots", {
                "operationType": NewBodyOperationType.REMOVE,
                "entities" : qSketchRegion(id + "fretSlotsProfile"),
                "direction" : evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "fretSlotsProfile", false) }).normal,
                "endBound" : BoundingType.UP_TO_NEXT,
                "isStartBoundOpposite" : true,
                "endTranslationalOffset" : -definition.blindSlotThickness,
                "startBound" : BoundingType.UP_TO_NEXT,
                "startTranslationalOffset" : -definition.blindSlotThickness
            });
    
            opBoolean(context, id + "fretSlotsCut", {
                    "tools" : qBodyType(qCreatedBy(id + "fretSlots", EntityType.BODY), BodyType.SOLID),
                    "targets": qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACTION
            });
            
            opDeleteBodies(context, id + "deleteSketchFretSlotsProfile", { "entities" : qCreatedBy(id + "fretSlotsProfile") });
            
            // Cut excess material after the last fret
            var excessCut = newSketch(context, id + "excessCutTop", {
                    "sketchPlane" : qCreatedBy(makeId("Top"), EntityType.FACE)
            });
            
            skRectangle(excessCut, "excessCutProfile", {
                    "firstCorner" : vector(-fretboardBottomWidth, definition.scaleLength),
                    "secondCorner" : vector(fretboardBottomWidth, fret_locations[definition.fretCount] + definition.bodyOverlap)
            });
            
            skSolve(excessCut);
            
            opExtrude(context, id + "excessCutBody", {
                    "operationType": NewBodyOperationType.REMOVE,
                    "entities" : qSketchRegion(id + "excessCutTop"),
                    "direction" : -evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "excessCutTop", false) }).normal,
                    "endBound" : BoundingType.THROUGH_ALL,
            });
            
            opBoolean(context, id + "excessCut", {
                    "targets" : qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "tools" : qBodyType(qCreatedBy(id + "excessCutBody", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACTION
            });
            
            opDeleteBodies(context, id + "deleteSketchExcessCutTop", { "entities" : qCreatedBy(id + "excessCutTop") });
            
            // Create fretboard radius
            var fretboardRadiusProfile = newSketch(context, id + "fretboardRadiusProfile", {
                "sketchPlane" : qCreatedBy(makeId("Front"), EntityType.FACE),
            });
            
            skCircle(fretboardRadiusProfile, "fretboardRadiusCircle", {
                    "center" : vector(0 * inch, -definition.fretboardRadius),
                    "radius" : definition.fretboardRadius
            });
            
            skSolve(fretboardRadiusProfile);
            
            opExtrude(context, id + "fretboardRadiusBody", {
                    "operationType": NewBodyOperationType.REMOVE,
                    "entities" : qSketchRegion(id + "fretboardRadiusProfile"),
                    "direction" : -evOwnerSketchPlane(context, { "entity" : qSketchRegion(id + "fretboardRadiusProfile", false) }).normal,
                    "endBound" : BoundingType.THROUGH_ALL
            });
            
                    
            opBoolean(context, id + "fretboardRadiusCut", {
                    "targets" : qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
                    "tools": qBodyType(qCreatedBy(id + "fretboardRadiusBody", EntityType.BODY), BodyType.SOLID),
                    "operationType" : BooleanOperationType.SUBTRACT_COMPLEMENT
            });
            
            opDeleteBodies(context, id + "deleteSketchFretboardRadiusProfile", { "entities" : qCreatedBy(id + "fretboardRadiusProfile") });
        }
        
        setProperty(context, {
            "entities" : qBodyType(qCreatedBy(id + "fretboardBlank", EntityType.BODY), BodyType.SOLID),
            "propertyType" : PropertyType.APPEARANCE,
            "value" : color(0.21, 0.21, 0.21)
        });
    });
