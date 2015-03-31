using System;
using System.Collections.Generic;
using DiffMatchPatch;

namespace Floobits.Common.Dmp
{
    public class FlooDmp : diff_match_patch {

        public new Object[] patch_apply(List<Patch> patches, string text) {
            if (patches.Count == 0) {
                return new Object[]{text, new bool[0]};
            }

            // Deep copy the patches so that no changes are made to originals.
            patches = patch_deepCopy(patches);

            string nullPadding = patch_addPadding(patches);
            int np_len = nullPadding.Length;
            text = nullPadding + text + nullPadding;
            patch_splitMax(patches);

            int x = 0;
            // delta keeps track of the offset between the expected and actual location
            // of the previous patch.  If there are patches expected at positions 10 and
            // 20, but the first patch was found at 12, delta is 2 and the second patch
            // has an effective expected position of 22.
            int delta = 0;
            bool[] results = new bool[patches.Count];
            Object[] positions = new FlooPatchPosition[patches.Count];

            foreach (Patch aPatch in patches) {
                FlooPatchPosition position = new FlooPatchPosition(3, 0, "");
                int expected_loc = aPatch.start2 + delta;
                String text1 = diff_text1(aPatch.diffs);
                int start_loc;
                int end_loc = -1;
                if (text1.Length > this.Match_MaxBits) {
                    // patch_splitMax will only provide an oversized pattern in the case of
                    // a monster delete.
                    start_loc = match_main(text,
                            text1.JavaSubstring(0, this.Match_MaxBits), expected_loc);
                    if (start_loc != -1) {
                        end_loc = match_main(text,
                                text1.Substring(text1.Length - this.Match_MaxBits),
                                expected_loc + text1.Length - this.Match_MaxBits);
                        if (end_loc == -1 || start_loc >= end_loc) {
                            // Can't find valid trailing context.  Drop this patch.
                            start_loc = -1;
                        }
                    }
                } else {
                    start_loc = match_main(text, text1, expected_loc);
                }
                if (start_loc == -1) {
                    // No match found.  :(
                    results[x] = false;
                    // Subtract the delta for this failed patch from subsequent patches.
                    delta -= aPatch.length2 - aPatch.length1;
                } else {
                    // Found a match.  :)
                    results[x] = true;
                    delta = start_loc - expected_loc;
                    String text2;
                    if (end_loc == -1) {
                        text2 = text.JavaSubstring(start_loc,
                                Math.Min(start_loc + text1.Length, text.Length));
                    } else {
                        text2 = text.JavaSubstring(start_loc,
                                Math.Min(end_loc + this.Match_MaxBits, text.Length));
                    }
                    if (text1.Equals(text2)) {
                        // Perfect match, just shove the replacement text in.
                        String replacement_str = diff_text2(aPatch.diffs);
                        text = text.JavaSubstring(0, start_loc) + replacement_str
                                + text.Substring(start_loc + text1.Length);
                        position = new FlooPatchPosition(start_loc, text1.Length, replacement_str);
                    } else {
                        // Imperfect match.  Run a diff to get a framework of equivalent
                        // indices.
                        List<Diff> diffs = diff_main(text1, text2, false);
                        if (text1.Length > this.Match_MaxBits
                                && diff_levenshtein(diffs) / (float) text1.Length
                                > this.Patch_DeleteThreshold) {
                            // The end points match, but the content is unacceptably bad.
                            results[x] = false;
                        } else {
                            diff_cleanupSemanticLossless(diffs);
                            int index1 = 0;
                            int delete_len = 0;
                            string inserted_text = "";
                            foreach (Diff aDiff in aPatch.diffs) {
                                if (aDiff.operation != Operation.EQUAL) {
                                    int index2 = diff_xIndex(diffs, index1);
                                    if (aDiff.operation == Operation.INSERT) {
                                        // Insertion
                                        text = text.JavaSubstring(0, start_loc + index2) + aDiff.text
                                                + text.Substring(start_loc + index2);
                                        inserted_text += aDiff.text;
                                    } else if (aDiff.operation == Operation.DELETE) {
                                        // Deletion
                                        int diff_index = diff_xIndex(diffs, index1 + aDiff.text.Length);
                                        text = text.JavaSubstring(0, start_loc + index2)
                                                + text.Substring(start_loc + diff_index);
                                        delete_len += (diff_index - index2);
                                    }
                                }
                                if (aDiff.operation != Operation.DELETE) {
                                    index1 += aDiff.text.Length;
                                }
                            }
                            position = new FlooPatchPosition(start_loc, delete_len, inserted_text);
                        }
                    }
                }
                int text_len = text.Length;
                if (position.start < np_len) {
                    position.end -= np_len - position.start;
                    position.text = position.text.Substring(Math.Min(np_len - position.start, position.text.Length));
                    position.start = 0;
                } else {
                    position.start -= np_len;
                }

                int too_close = (position.start + position.text.Length) - (text_len - 2 * np_len);
                if (too_close > 0){

                    int start = position.text.Length - too_close;
                    start = start < 0 ? 0 : start;
                    position.text = position.text.JavaSubstring(0, start);
                }
                positions[x] = position;
                x++;

            }
            // Strip the padding off.
            text = text.JavaSubstring(nullPadding.Length, text.Length
                    - nullPadding.Length);
            return new Object[]{text, results, positions};
        }
    }
}
